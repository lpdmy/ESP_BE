using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using EduShpere.MLTrainer.Models;
using EduShpere.Infrastructure;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.MLTrainer.Services;

/// <summary>
/// Service để train và sử dụng ML model dự đoán slot thời gian phù hợp
/// </summary>
public class ScheduleMLService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<SchedulePredictionData, SchedulePrediction>? _predictionEngine;
    private readonly string _modelPath;

    public ScheduleMLService()
    {
        _mlContext = new MLContext(seed: 0);
        _modelPath = Path.Combine("Data", "schedule_prediction_model.zip");
    }

    /// <summary>
    /// Train model từ dữ liệu database
    /// </summary>
    public async Task TrainModelAsync(EduShpereDbContext dbContext)
    {
        Console.WriteLine("📊 Đang thu thập dữ liệu từ database...");

        // Lấy dữ liệu từ Activities và ActivityParticipants
        var activities = await dbContext.Activities
            .Include(a => a.ActivityParticipants)
            .Where(a => !a.IsDeleted && a.StartDate.HasValue)
            .ToListAsync();

        var trainingData = new List<SchedulePredictionData>();

        foreach (var activity in activities)
        {
            if (!activity.StartDate.HasValue) continue;

            var startDate = activity.StartDate.Value;
            var hourOfDay = startDate.Hour;
            var dayOfWeek = (int)startDate.DayOfWeek;
            var activityType = (int)activity.Category;

            foreach (var participant in activity.ActivityParticipants.Where(p => p.Status == Domain.Enum.ParticipantStatus.Joined))
            {
                // Tính toán features cho mỗi user
                var userActivities = activities
                    .Where(a => a.ActivityParticipants.Any(p => p.UserId == participant.UserId && p.Status == Domain.Enum.ParticipantStatus.Joined))
                    .ToList();

                var previousCount = userActivities.Count(a => 
                    a.StartDate.HasValue && 
                    a.StartDate.Value >= startDate.AddDays(-7) && 
                    a.StartDate.Value < startDate);

                var preferredHour = userActivities
                    .Where(a => a.StartDate.HasValue)
                    .Select(a => (float)a.StartDate.Value.Hour)
                    .DefaultIfEmpty(9)
                    .Average();

                // Tính score dựa trên:
                // - User đã tham gia activity này (score = 1)
                // - Hoặc dựa trên pattern tương tự (score = 0.5-0.9)
                var score = 1.0f; // User đã tham gia = perfect match

                trainingData.Add(new SchedulePredictionData
                {
                    UserId = (float)participant.UserId,
                    HourOfDay = (float)hourOfDay,
                    DayOfWeek = (float)dayOfWeek,
                    ActivityType = (float)activityType,
                    PreviousActivityCount = (float)previousCount,
                    PreferredHour = preferredHour,
                    Score = score
                });
            }
        }

        if (trainingData.Count == 0)
        {
            Console.WriteLine("⚠️ Không có dữ liệu để train. Sử dụng model mặc định.");
            return;
        }

        Console.WriteLine($"✅ Đã thu thập {trainingData.Count} mẫu dữ liệu");

        // Load dữ liệu vào ML.NET
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Chia train/test split
        var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        // Tạo pipeline - Tất cả columns đã là float nên có thể concatenate trực tiếp
        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(SchedulePredictionData.UserId),
                nameof(SchedulePredictionData.HourOfDay),
                nameof(SchedulePredictionData.DayOfWeek),
                nameof(SchedulePredictionData.ActivityType),
                nameof(SchedulePredictionData.PreviousActivityCount),
                nameof(SchedulePredictionData.PreferredHour))
            .Append(_mlContext.Regression.Trainers.Sdca(
                nameof(SchedulePredictionData.Score),
                "Features",
                maximumNumberOfIterations: 100));

        // Train model
        Console.WriteLine("🤖 Đang train model...");
        _model = pipeline.Fit(trainTestSplit.TrainSet);

        // Evaluate
        var predictions = _model.Transform(trainTestSplit.TestSet);
        var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(SchedulePredictionData.Score));

        Console.WriteLine($"📈 Model Metrics:");
        Console.WriteLine($"   R² Score: {metrics.RSquared:F4}");
        Console.WriteLine($"   MSE: {metrics.MeanSquaredError:F4}");
        Console.WriteLine($"   MAE: {metrics.MeanAbsoluteError:F4}");

        // Lưu model
        Directory.CreateDirectory(Path.GetDirectoryName(_modelPath) ?? "Data");
        _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
        Console.WriteLine($"✅ Model đã được lưu tại: {_modelPath}");

        // Tạo prediction engine
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<SchedulePredictionData, SchedulePrediction>(_model);
    }

    /// <summary>
    /// Load model đã train
    /// </summary>
    public void LoadModel()
    {
        if (!File.Exists(_modelPath))
        {
            Console.WriteLine("⚠️ Model chưa được train. Vui lòng train model trước.");
            return;
        }

        DataViewSchema schema;
        using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            _model = _mlContext.Model.Load(stream, out schema);
        }

        _predictionEngine = _mlContext.Model.CreatePredictionEngine<SchedulePredictionData, SchedulePrediction>(_model);
        Console.WriteLine("✅ Model đã được load");
    }

    /// <summary>
    /// Dự đoán điểm số phù hợp của một slot thời gian cho user
    /// </summary>
    public float PredictScore(int userId, int hourOfDay, int dayOfWeek, int activityType, int previousActivityCount, float preferredHour)
    {
        if (_predictionEngine == null)
        {
            LoadModel();
            if (_predictionEngine == null)
            {
                // Trả về score mặc định nếu chưa có model
                return 0.5f;
            }
        }

        var input = new SchedulePredictionData
        {
            UserId = (float)userId,
            HourOfDay = (float)hourOfDay,
            DayOfWeek = (float)dayOfWeek,
            ActivityType = (float)activityType,
            PreviousActivityCount = (float)previousActivityCount,
            PreferredHour = preferredHour,
            Score = 0 // Không cần cho prediction
        };

        var prediction = _predictionEngine.Predict(input);
        return Math.Max(0, Math.Min(1, prediction.PredictedScore)); // Clamp 0-1
    }

    /// <summary>
    /// Dự đoán các slot thời gian phù hợp nhất cho user
    /// </summary>
    public List<(int Hour, int DayOfWeek, float Score)> PredictBestSlots(
        int userId, 
        int activityType, 
        int previousActivityCount, 
        float preferredHour,
        DateTime startDate,
        DateTime endDate)
    {
        var slots = new List<(int Hour, int DayOfWeek, float Score)>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayOfWeek = (int)date.DayOfWeek;
            
            // Dự đoán cho các giờ trong ngày (8h-20h)
            for (int hour = 8; hour <= 20; hour++)
            {
                var score = PredictScore(userId, hour, dayOfWeek, activityType, previousActivityCount, preferredHour);
                slots.Add((hour, dayOfWeek, score));
            }
        }

        // Sắp xếp theo score giảm dần
        return slots.OrderByDescending(s => s.Score).ToList();
    }
}

