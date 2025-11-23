using Microsoft.ML;
using Microsoft.ML.Data;
using EduShpere.MLTrainer.Models;
using EduShpere.Infrastructure;
using EduShpere.Domain.Models;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EduShpere.MLTrainer.Services;

/// <summary>
/// Service để train và sử dụng ML model dự đoán lịch cho activity mới
/// Dựa trên pattern của các activities tương tự đã có
/// </summary>
public class ActivityScheduleMLService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<ActivityScheduleData, ActivitySchedulePrediction>? _predictionEngine;
    private readonly string _modelPath;

    public ActivityScheduleMLService()
    {
        _mlContext = new MLContext(seed: 0);
        _modelPath = Path.Combine("Data", "activity_schedule_model.zip");
    }

    /// <summary>
    /// Hash string thành float (đơn giản)
    /// </summary>
    private float HashString(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;
        var hash = input.GetHashCode();
        return Math.Abs(hash % 10000) / 10000f; // Normalize về 0-1
    }

    /// <summary>
    /// Train model từ dữ liệu database
    /// </summary>
    public async Task TrainModelAsync(EduShpereDbContext dbContext)
    {
        Console.WriteLine("📊 Đang thu thập dữ liệu từ database...");

        // Lấy tất cả activities đã có StartDate và có participants
        var activities = await dbContext.Activities
            .Include(a => a.ActivityParticipants)
            .Where(a => !a.IsDeleted && 
                       a.StartDate.HasValue && 
                       a.EndDate.HasValue &&
                       a.ActivityParticipants.Any())
            .ToListAsync();

        var trainingData = new List<ActivityScheduleData>();

        foreach (var activity in activities)
        {
            if (!activity.StartDate.HasValue) continue;

            var startDate = activity.StartDate.Value;
            var hourOfDay = (float)startDate.Hour;
            var dayOfWeek = (float)(int)startDate.DayOfWeek;
            var month = (float)startDate.Month;
            var activityType = (float)(int)activity.Category;

            // Tính số participants thực tế
            var participantCount = activity.ActivityParticipants
                .Count(p => p.Status == Domain.Enum.ParticipantStatus.Joined);

            // Tính success score: tỷ lệ tham gia
            var successScore = activity.MaxParticipants > 0
                ? Math.Min(1.0f, (float)participantCount / activity.MaxParticipants)
                : 0.5f; // Nếu không có max, dùng score trung bình

            trainingData.Add(new ActivityScheduleData
            {
                ActivityType = activityType,
                SubTypeHash = HashString(activity.SubType ?? ""),
                HourOfDay = hourOfDay,
                DayOfWeek = dayOfWeek,
                Month = month,
                LocationHash = HashString(activity.Location ?? ""),
                ParticipantCount = participantCount,
                MaxParticipants = activity.MaxParticipants,
                SuccessScore = successScore
            });
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

        // Tạo pipeline
        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(ActivityScheduleData.ActivityType),
                nameof(ActivityScheduleData.SubTypeHash),
                nameof(ActivityScheduleData.HourOfDay),
                nameof(ActivityScheduleData.DayOfWeek),
                nameof(ActivityScheduleData.Month),
                nameof(ActivityScheduleData.LocationHash),
                nameof(ActivityScheduleData.MaxParticipants))
            .Append(_mlContext.Regression.Trainers.Sdca(
                nameof(ActivityScheduleData.SuccessScore),
                "Features",
                maximumNumberOfIterations: 100));

        // Train model
        Console.WriteLine("🤖 Đang train model...");
        _model = pipeline.Fit(trainTestSplit.TrainSet);

        // Evaluate
        var predictions = _model.Transform(trainTestSplit.TestSet);
        var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(ActivityScheduleData.SuccessScore));

        Console.WriteLine($"📈 Model Metrics:");
        Console.WriteLine($"   R² Score: {metrics.RSquared:F4}");
        Console.WriteLine($"   MSE: {metrics.MeanSquaredError:F4}");
        Console.WriteLine($"   MAE: {metrics.MeanAbsoluteError:F4}");

        // Lưu model
        Directory.CreateDirectory(Path.GetDirectoryName(_modelPath) ?? "Data");
        _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
        Console.WriteLine($"✅ Model đã được lưu tại: {_modelPath}");

        // Tạo prediction engine
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<ActivityScheduleData, ActivitySchedulePrediction>(_model);
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

        _predictionEngine = _mlContext.Model.CreatePredictionEngine<ActivityScheduleData, ActivitySchedulePrediction>(_model);
        Console.WriteLine("✅ Model đã được load");
    }

    /// <summary>
    /// Dự đoán success score cho một slot thời gian của activity
    /// </summary>
    public float PredictSuccessScore(
        ActivityType activityType,
        string subType,
        int hourOfDay,
        int dayOfWeek,
        int month,
        string? location,
        int maxParticipants)
    {
        if (_predictionEngine == null)
        {
            LoadModel();
            if (_predictionEngine == null)
            {
                return 0.5f; // Score mặc định
            }
        }

        var input = new ActivityScheduleData
        {
            ActivityType = (float)(int)activityType,
            SubTypeHash = HashString(subType),
            HourOfDay = hourOfDay,
            DayOfWeek = dayOfWeek,
            Month = month,
            LocationHash = HashString(location ?? ""),
            ParticipantCount = 0, // Không cần cho prediction
            MaxParticipants = maxParticipants,
            SuccessScore = 0 // Không cần cho prediction
        };

        var prediction = _predictionEngine.Predict(input);
        return Math.Max(0, Math.Min(1, prediction.PredictedSuccessScore));
    }

    /// <summary>
    /// Dự đoán các slot thời gian tốt nhất cho activity mới
    /// </summary>
    public List<(DateTime StartTime, float Score)> PredictBestTimeSlots(
        ActivityType activityType,
        string subType,
        string? location,
        int maxParticipants,
        DateTime startDate,
        DateTime endDate)
    {
        var slots = new List<(DateTime StartTime, float Score)>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var month = date.Month;

            // Dự đoán cho các giờ trong ngày (8h-20h)
            for (int hour = 8; hour <= 20; hour++)
            {
                var slotStart = date.AddHours(hour);
                var score = PredictSuccessScore(
                    activityType,
                    subType,
                    hour,
                    dayOfWeek,
                    month,
                    location,
                    maxParticipants);

                slots.Add((slotStart, score));
            }
        }

        // Sắp xếp theo score giảm dần
        return slots.OrderByDescending(s => s.Score).ToList();
    }
}

