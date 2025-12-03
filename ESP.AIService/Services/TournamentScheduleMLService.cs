using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using ESP.AIService.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AiActivityMatch = ESP.AIService.Entities.ActivityMatch;
using AiMatchStatus = ESP.AIService.Entities.MatchStatus;

namespace ESP.AIService.Services;

public class TournamentScheduleMLService
{
    private readonly MLContext _mlContext;
    private readonly string _modelPath;
    private ITransformer? _model;

    public TournamentScheduleMLService()
    {
        _mlContext = new MLContext(seed: 0);
        
        // Tìm thư mục Data: ưu tiên trong output directory, sau đó là project directory
        var baseDir = AppContext.BaseDirectory;
        var dataDir = Path.Combine(baseDir, "Data");
        
        // Nếu không có trong output directory, thử tìm trong project directory
        if (!Directory.Exists(dataDir))
        {
            var projectDataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (Directory.Exists(projectDataDir))
            {
                dataDir = projectDataDir;
            }
            else
            {
                // Tạo thư mục mới
                Directory.CreateDirectory(dataDir);
            }
        }
        
        _modelPath = Path.Combine(dataDir, "tournament_schedule_model.zip");
        
        Console.WriteLine($"📁 Model path: {_modelPath}");
        Console.WriteLine($"   Base directory: {baseDir}");
        Console.WriteLine($"   Data directory exists: {Directory.Exists(dataDir)}");
        Console.WriteLine($"   Model file exists: {File.Exists(_modelPath)}");
    }

    /// <summary>
    /// Train model từ dữ liệu matches đã có
    /// </summary>
    public void TrainModel(EduShpereDbContext dbContext)
    {
        // Lấy tất cả matches đã hoàn thành từ database
        // Sử dụng raw SQL để query từ table ActivityMatches
        List<AiActivityMatch> matches = new();
        
        try
        {
            // Query trực tiếp từ table ActivityMatches bằng raw SQL
            // Chỉ select các columns thực sự có trong database
            // Cast các cột để đảm bảo kiểu dữ liệu đúng (tránh lỗi byte -> int)
            var sql = @"
                SELECT CAST(Id AS INT) AS Id, 
                       CAST(ActivityId AS INT) AS ActivityId, 
                       CAST(SportId AS INT) AS SportId, 
                       CAST(ClassGroup1Id AS INT) AS ClassGroup1Id, 
                       CAST(ClassGroup2Id AS INT) AS ClassGroup2Id, 
                       CAST(Grade AS INT) AS Grade, 
                       MatchDate, StartTime, EndTime, Location, 
                       CAST(Status AS INT) AS Status, 
                       CAST(Score1 AS INT) AS Score1, 
                       CAST(Score2 AS INT) AS Score2,
                       CAST(WinnerClassGroupId AS INT) AS WinnerClassGroupId, 
                       CAST(Round AS INT) AS Round, 
                       RoundName, 
                       CAST(MatchNumber AS INT) AS MatchNumber, 
                       CAST(NextMatchId AS INT) AS NextMatchId, 
                       CAST(IsBye AS BIT) AS IsBye, 
                       Notes, CreatedAt, 
                       CAST(CreatedBy AS INT) AS CreatedBy, 
                       UpdatedAt, 
                       CAST(UpdatedBy AS INT) AS UpdatedBy, 
                       CAST(IsDeleted AS BIT) AS IsDeleted
                FROM ActivityMatches 
                WHERE Status = {0} 
                  AND MatchDate IS NOT NULL 
                  AND StartTime IS NOT NULL 
                  AND EndTime IS NOT NULL
                  AND IsDeleted = 0";
            
            var matchDtos = dbContext.Database
                .SqlQueryRaw<Models.ActivityMatchDto>(sql, (int)AiMatchStatus.Completed)
                .ToList();
            
            matches = matchDtos.Select(dto => dto.ToActivityMatch()).ToList();
        }
        catch (Exception ex)
        {
            // Nếu không có table hoặc có lỗi
            Console.WriteLine($"⚠️ Không thể query ActivityMatches: {ex.Message}");
            Console.WriteLine("   Table có thể chưa được tạo. Sẽ train với dữ liệu mẫu hoặc bỏ qua.");
            matches = new List<AiActivityMatch>();
        }

        if (matches.Count < 20)
        {
            Console.WriteLine($"⚠️ Không đủ dữ liệu để train. Cần ít nhất 20 matches, hiện có: {matches.Count}");
            return;
        }

        // Tạo training data
        var trainingData = new List<TournamentScheduleData>();
        
        foreach (var match in matches)
        {
            if (!match.MatchDate.HasValue || !match.StartTime.HasValue || !match.EndTime.HasValue)
                continue;

            var matchDate = match.MatchDate.Value;
            var startTime = match.StartTime.Value;
            var duration = match.EndTime.Value - startTime;
            
            // Tính success score dựa trên:
            // - Match có diễn ra đúng giờ không (Status = Completed)
            // - Có conflict không (giả sử nếu completed = không conflict)
            // - Có đủ thời gian không (duration hợp lý)
            var successScore = CalculateSuccessScore(match, duration);

            var data = new TournamentScheduleData
            {
                ActivityId = (float)match.ActivityId,
                SportId = (float)match.SportId,
                NumberOfTeams = 2.0f, // Mặc định 2 teams
                NumberOfRounds = (float)match.Round,
                HourOfDay = (float)startTime.Hours,
                DayOfWeek = (float)matchDate.DayOfWeek,
                Month = (float)matchDate.Month,
                DayOfMonth = (float)matchDate.Day,
                Round = (float)match.Round,
                MatchNumber = (float)match.MatchNumber,
                Grade = match.Grade.HasValue ? (float)match.Grade.Value : 0.0f,
                LocationHash = (float)(match.Location?.GetHashCode() ?? 0),
                MatchDurationMinutes = (float)duration.TotalMinutes,
                SuccessScore = successScore
            };

            trainingData.Add(data);
        }

        if (trainingData.Count == 0)
        {
            Console.WriteLine("⚠️ Không có dữ liệu training hợp lệ.");
            return;
        }

        // Load data vào ML.NET
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Tạo pipeline
        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(TournamentScheduleData.ActivityId),
                nameof(TournamentScheduleData.SportId),
                nameof(TournamentScheduleData.NumberOfTeams),
                nameof(TournamentScheduleData.NumberOfRounds),
                nameof(TournamentScheduleData.HourOfDay),
                nameof(TournamentScheduleData.DayOfWeek),
                nameof(TournamentScheduleData.Month),
                nameof(TournamentScheduleData.DayOfMonth),
                nameof(TournamentScheduleData.Round),
                nameof(TournamentScheduleData.MatchNumber),
                nameof(TournamentScheduleData.Grade),
                nameof(TournamentScheduleData.LocationHash),
                nameof(TournamentScheduleData.MatchDurationMinutes)
            )
            .Append(_mlContext.Regression.Trainers.Sdca(
                labelColumnName: nameof(TournamentScheduleData.SuccessScore),
                featureColumnName: "Features",
                maximumNumberOfIterations: 100));

        // Train model
        Console.WriteLine($"🔄 Training model với {trainingData.Count} samples...");
        _model = pipeline.Fit(dataView);

        // Evaluate model
        var predictions = _model.Transform(dataView);
        var metrics = _mlContext.Regression.Evaluate(predictions, 
            labelColumnName: nameof(TournamentScheduleData.SuccessScore),
            scoreColumnName: "Score");

        Console.WriteLine($"✅ Model trained!");
        Console.WriteLine($"   R² Score: {metrics.RSquared:F4}");
        Console.WriteLine($"   Loss: {metrics.MeanSquaredError:F4}");

        // Save model
        _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
        Console.WriteLine($"💾 Model saved to: {_modelPath}");
    }

    /// <summary>
    /// Dự đoán success score cho một slot
    /// </summary>
    public float PredictSuccessScore(TournamentScheduleData data)
    {
        LoadModelIfNeeded();

        if (_model == null)
        {
            Console.WriteLine("⚠️ Model chưa được train. Trả về score mặc định 0.5");
            return 0.5f;
        }

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<TournamentScheduleData, TournamentSchedulePrediction>(_model);
        var prediction = predictionEngine.Predict(data);
        
        // Đảm bảo score trong khoảng 0-1
        return Math.Max(0, Math.Min(1, prediction.PredictedSuccessScore));
    }

    /// <summary>
    /// Tạo danh sách slots với ML scores
    /// </summary>
    public List<ScheduleSlot> GenerateSlotsWithScores(
        TournamentScheduleRequest request,
        EduShpereDbContext dbContext)
    {
        LoadModelIfNeeded();

        var slots = new List<ScheduleSlot>();
        var startTime = request.PreferredStartTime ?? TimeSpan.FromHours(8);
        var endTime = request.PreferredEndTime ?? TimeSpan.FromHours(17);
        var currentDate = request.StartDate.Date;

        // Nếu không cấu hình sân, vẫn tạo slot với Location = null (giống cũ)
        var locations = request.AvailableLocations != null && request.AvailableLocations.Any()
            ? request.AvailableLocations
            : new List<string?> { null };

        while (currentDate <= request.EndDate.Date)
        {
            var currentTime = startTime;
            
            while (currentTime + request.MatchDuration <= endTime)
            {
                var slotEndTime = currentTime + request.MatchDuration;

                foreach (var loc in locations)
                {
                    // Tạo data để predict – LocationHash theo từng sân
                    var data = new TournamentScheduleData
                    {
                        ActivityId = (float)request.ActivityId,
                        SportId = (float)request.SportId,
                        NumberOfTeams = (float)request.ClassGroupIds.Count,
                        NumberOfRounds = 1.0f, // Sẽ được tính sau
                        HourOfDay = (float)currentTime.Hours,
                        DayOfWeek = (float)currentDate.DayOfWeek,
                        Month = (float)currentDate.Month,
                        DayOfMonth = (float)currentDate.Day,
                        Round = 1.0f,
                        MatchNumber = 1.0f,
                        Grade = request.Grade.HasValue ? (float)request.Grade.Value : 0.0f,
                        LocationHash = loc != null ? (float)loc.GetHashCode() : 0.0f,
                        MatchDurationMinutes = (float)request.MatchDuration.TotalMinutes,
                        SuccessScore = 0.0f // Không cần cho prediction
                    };

                    var mlScore = PredictSuccessScore(data);

                    // Điều chỉnh score dựa trên UserNotes
                    var adjustedScore = AdjustScoreByUserNotes(mlScore, request.UserNotes, currentTime, currentDate);

                    var slot = new ScheduleSlot
                    {
                        MatchDate = currentDate,
                        StartTime = currentTime,
                        EndTime = slotEndTime,
                        Location = loc,
                        MLScore = adjustedScore,
                        Explanation = $"ML Score: {adjustedScore:F2} - {currentDate:dd/MM/yyyy} {currentTime:hh\\:mm} - Sân: {loc ?? "N/A"}"
                    };

                    slots.Add(slot);
                }

                // Tăng thời gian với gap (trên cùng một sân)
                currentTime = currentTime.Add(TimeSpan.FromMinutes(request.MatchDuration.TotalMinutes + request.MinGapBetweenMatches));
            }

            currentDate = currentDate.AddDays(1);
        }

        // Sắp xếp theo ML score giảm dần
        return slots.OrderByDescending(s => s.MLScore).ToList();
    }

    /// <summary>
    /// Điều chỉnh ML score dựa trên UserNotes sử dụng NLP và weighted scoring
    /// </summary>
    private float AdjustScoreByUserNotes(float baseScore, string? userNotes, TimeSpan time, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(userNotes))
            return baseScore;

        var notes = userNotes.ToLowerInvariant().Trim();
        var adjustedScore = baseScore;
        
        // Tokenize và normalize notes
        var tokens = TokenizeAndNormalize(notes);
        
        // Phân tích sentiment và intent
        var preferences = AnalyzePreferences(tokens, notes);
        
        // Tính toán adjustment score dựa trên nhiều yếu tố
        var timeAdjustment = CalculateTimePreferenceScore(preferences, time);
        var dayAdjustment = CalculateDayPreferenceScore(preferences, date);
        var contextAdjustment = CalculateContextScore(preferences, time, date);
        
        // Weighted combination
        var totalAdjustment = (timeAdjustment * 0.4f) + (dayAdjustment * 0.3f) + (contextAdjustment * 0.3f);
        
        // Apply adjustment với sigmoid để smooth
        adjustedScore = ApplySigmoidAdjustment(baseScore, totalAdjustment);
        
        // Đảm bảo score trong range [0, 1]
        return Math.Max(0.0f, Math.Min(1.0f, adjustedScore));
    }

    /// <summary>
    /// Tokenize và normalize text
    /// </summary>
    private List<string> TokenizeAndNormalize(string text)
    {
        var tokens = new List<string>();
        var words = text.Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\n', '\r', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            var normalized = word.Trim().ToLowerInvariant();
            if (normalized.Length > 0)
            {
                tokens.Add(normalized);
            }
        }
        
        return tokens;
    }

    /// <summary>
    /// Phân tích preferences từ tokens và text
    /// </summary>
    private PreferenceAnalysis AnalyzePreferences(List<string> tokens, string fullText)
    {
        var analysis = new PreferenceAnalysis();
        
        // Keywords với weights
        var timeKeywords = new Dictionary<string, (TimePreference preference, float weight)>
        {
            { "sáng", (TimePreference.Morning, 1.0f) },
            { "buổi sáng", (TimePreference.Morning, 1.2f) },
            { "morning", (TimePreference.Morning, 1.0f) },
            { "chiều", (TimePreference.Afternoon, 1.0f) },
            { "buổi chiều", (TimePreference.Afternoon, 1.2f) },
            { "afternoon", (TimePreference.Afternoon, 1.0f) },
            { "tối", (TimePreference.Evening, 1.0f) },
            { "buổi tối", (TimePreference.Evening, 1.2f) },
            { "evening", (TimePreference.Evening, 1.0f) },
            { "night", (TimePreference.Evening, 1.0f) },
            { "đêm", (TimePreference.Evening, 1.0f) },
        };
        
        var dayKeywords = new Dictionary<string, (DayPreference preference, float weight)>
        {
            { "cuối tuần", (DayPreference.Weekend, 1.2f) },
            { "weekend", (DayPreference.Weekend, 1.0f) },
            { "thứ 7", (DayPreference.Weekend, 0.8f) },
            { "chủ nhật", (DayPreference.Weekend, 0.8f) },
            { "trong tuần", (DayPreference.Weekday, 1.2f) },
            { "weekday", (DayPreference.Weekday, 1.0f) },
            { "thứ 2", (DayPreference.Weekday, 0.3f) },
            { "thứ 3", (DayPreference.Weekday, 0.3f) },
            { "thứ 4", (DayPreference.Weekday, 0.3f) },
            { "thứ 5", (DayPreference.Weekday, 0.3f) },
            { "thứ 6", (DayPreference.Weekday, 0.3f) },
        };
        
        var negativeKeywords = new[] { "tránh", "không", "không muốn", "avoid", "no", "not", "không thích" };
        var positiveKeywords = new[] { "ưu tiên", "thích", "tốt", "prefer", "like", "good", "best" };
        
        // Phát hiện negative/positive sentiment
        bool isNegative = false;
        bool isPositive = false;
        
        foreach (var token in tokens)
        {
            if (negativeKeywords.Any(k => token.Contains(k)))
                isNegative = true;
            if (positiveKeywords.Any(k => token.Contains(k)))
                isPositive = true;
        }
        
        // Nếu không có explicit sentiment, mặc định là positive
        if (!isNegative && !isPositive)
            isPositive = true;
        
        // Phân tích time preference
        foreach (var kvp in timeKeywords)
        {
            if (fullText.Contains(kvp.Key))
            {
                var weight = kvp.Value.weight;
                if (isNegative) weight *= -1.0f;
                
                if (!analysis.TimePreferences.ContainsKey(kvp.Value.preference))
                    analysis.TimePreferences[kvp.Value.preference] = 0.0f;
                
                analysis.TimePreferences[kvp.Value.preference] += weight;
            }
        }
        
        // Phân tích day preference
        foreach (var kvp in dayKeywords)
        {
            if (fullText.Contains(kvp.Key))
            {
                var weight = kvp.Value.weight;
                if (isNegative) weight *= -1.0f;
                
                if (!analysis.DayPreferences.ContainsKey(kvp.Value.preference))
                    analysis.DayPreferences[kvp.Value.preference] = 0.0f;
                
                analysis.DayPreferences[kvp.Value.preference] += weight;
            }
        }
        
        // Phát hiện giờ cao điểm
        if (fullText.Contains("cao điểm") || fullText.Contains("rush hour") || 
            fullText.Contains("giờ tan học") || fullText.Contains("giờ tan ca"))
        {
            analysis.AvoidRushHour = !isNegative; // Nếu có "tránh" thì avoid = true
        }
        
        // Phát hiện specific time ranges
        var timePatterns = new System.Text.RegularExpressions.Regex(@"(\d{1,2})\s*(h|giờ|:)\s*(\d{0,2})");
        var matches = timePatterns.Matches(fullText);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (int.TryParse(match.Groups[1].Value, out int hour))
            {
                analysis.SpecificHours.Add(hour);
            }
        }
        
        return analysis;
    }

    /// <summary>
    /// Tính toán time preference score
    /// </summary>
    private float CalculateTimePreferenceScore(PreferenceAnalysis preferences, TimeSpan time)
    {
        float score = 0.0f;
        var hour = time.Hours;
        
        // Check specific hours
        if (preferences.SpecificHours.Any())
        {
            var closestHour = preferences.SpecificHours.OrderBy(h => Math.Abs(h - hour)).First();
            var distance = Math.Abs(closestHour - hour);
            if (distance == 0)
                score += 0.3f;
            else if (distance <= 1)
                score += 0.2f * (1.0f - distance * 0.5f);
        }
        
        // Check time preferences với weights
        if (preferences.TimePreferences.ContainsKey(TimePreference.Morning))
        {
            var weight = preferences.TimePreferences[TimePreference.Morning];
            if (hour >= 6 && hour < 12)
                score += weight * 0.25f;
            else
                score -= Math.Abs(weight) * 0.15f;
        }
        
        if (preferences.TimePreferences.ContainsKey(TimePreference.Afternoon))
        {
            var weight = preferences.TimePreferences[TimePreference.Afternoon];
            if (hour >= 12 && hour < 18)
                score += weight * 0.25f;
            else
                score -= Math.Abs(weight) * 0.15f;
        }
        
        if (preferences.TimePreferences.ContainsKey(TimePreference.Evening))
        {
            var weight = preferences.TimePreferences[TimePreference.Evening];
            if (hour >= 18 || hour < 6)
                score += weight * 0.25f;
            else
                score -= Math.Abs(weight) * 0.15f;
        }
        
        // Avoid rush hour
        if (preferences.AvoidRushHour)
        {
            if ((hour >= 7 && hour < 9) || (hour >= 17 && hour < 19))
                score -= 0.3f;
            else
                score += 0.1f;
        }
        
        return Math.Max(-0.5f, Math.Min(0.5f, score)); // Limit adjustment range
    }

    /// <summary>
    /// Tính toán day preference score
    /// </summary>
    private float CalculateDayPreferenceScore(PreferenceAnalysis preferences, DateTime date)
    {
        float score = 0.0f;
        var dayOfWeek = date.DayOfWeek;
        var isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
        var isWeekday = dayOfWeek >= DayOfWeek.Monday && dayOfWeek <= DayOfWeek.Friday;
        
        if (preferences.DayPreferences.ContainsKey(DayPreference.Weekend))
        {
            var weight = preferences.DayPreferences[DayPreference.Weekend];
            if (isWeekend)
                score += weight * 0.3f;
            else
                score -= Math.Abs(weight) * 0.2f;
        }
        
        if (preferences.DayPreferences.ContainsKey(DayPreference.Weekday))
        {
            var weight = preferences.DayPreferences[DayPreference.Weekday];
            if (isWeekday)
                score += weight * 0.3f;
            else
                score -= Math.Abs(weight) * 0.2f;
        }
        
        return Math.Max(-0.5f, Math.Min(0.5f, score));
    }

    /// <summary>
    /// Tính toán context score (kết hợp time và day)
    /// </summary>
    private float CalculateContextScore(PreferenceAnalysis preferences, TimeSpan time, DateTime date)
    {
        float score = 0.0f;
        
        // Nếu có nhiều preferences, boost score nếu match nhiều điều kiện
        var matchCount = 0;
        
        if (preferences.TimePreferences.Any())
            matchCount++;
        if (preferences.DayPreferences.Any())
            matchCount++;
        if (preferences.SpecificHours.Any())
            matchCount++;
        
        // Synergy bonus: nếu match nhiều preferences
        if (matchCount >= 2)
            score += 0.1f;
        
        return Math.Max(-0.2f, Math.Min(0.2f, score));
    }

    /// <summary>
    /// Áp dụng sigmoid adjustment để smooth transition
    /// </summary>
    private float ApplySigmoidAdjustment(float baseScore, float adjustment)
    {
        // Sigmoid function để smooth adjustment
        var sigmoid = 1.0f / (1.0f + (float)Math.Exp(-adjustment * 5.0f));
        
        // Map từ [0,1] sang [-0.3, 0.3] adjustment range
        var mappedAdjustment = (sigmoid - 0.5f) * 0.6f;
        
        return baseScore + mappedAdjustment;
    }

    // Helper classes
    private class PreferenceAnalysis
    {
        public Dictionary<TimePreference, float> TimePreferences { get; set; } = new();
        public Dictionary<DayPreference, float> DayPreferences { get; set; } = new();
        public bool AvoidRushHour { get; set; } = false;
        public List<int> SpecificHours { get; set; } = new();
    }

    private enum TimePreference
    {
        Morning,
        Afternoon,
        Evening
    }

    private enum DayPreference
    {
        Weekend,
        Weekday
    }

    private void LoadModelIfNeeded()
    {
        if (_model == null)
        {
            if (File.Exists(_modelPath))
            {
                try
                {
                    var dataView = _mlContext.Data.LoadFromEnumerable(new List<TournamentScheduleData>());
                    _model = _mlContext.Model.Load(_modelPath, out var schema);
                    Console.WriteLine($"✅ Model loaded from: {_modelPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Không thể load model: {ex.Message}");
                    Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                    _model = null; // Đảm bảo _model = null để dùng default score
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Model file không tồn tại: {_modelPath}");
                Console.WriteLine($"   Sẽ sử dụng default score (0.5) cho tất cả slots.");
                Console.WriteLine($"   Để train model, gọi API: POST /api/activity/train-schedule-model");
            }
        }
    }

    private float CalculateSuccessScore(AiActivityMatch match, TimeSpan duration)
    {
        // Tính success score dựa trên các yếu tố:
        // 1. Match đã completed = 0.8 điểm
        // 2. Duration hợp lý (30 phút - 2 giờ) = 0.2 điểm
        float score = 0.0f;

        if (match.Status == AiMatchStatus.Completed)
        {
            score += 0.8f;
        }

        var durationMinutes = duration.TotalMinutes;
        if (durationMinutes >= 30 && durationMinutes <= 120)
        {
            score += 0.2f;
        }

        return Math.Min(1.0f, score);
    }
}

