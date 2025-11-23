using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using EduShpere.MLTrainer.Services;
using EduShpere.MLTrainer.Workers;
using EduShpere.Infrastructure;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.MLTrainer;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 EduShpere ML Trainer - AI Scheduling System");
        Console.WriteLine("================================================\n");

        // Cấu hình
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ Connection string không được tìm thấy!");
            return;
        }

        // Tạo DbContext
        var optionsBuilder = new DbContextOptionsBuilder<EduShpereDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        using var dbContext = new EduShpereDbContext(optionsBuilder.Options);

        // Menu chọn chức năng
        Console.WriteLine("Chọn chức năng:");
        Console.WriteLine("1. Train Activity Classification Model (Phân loại hoạt động)");
        Console.WriteLine("2. Train Schedule Prediction Model (Dự đoán lịch cho user) - CŨ");
        Console.WriteLine("3. Train Activity Schedule Model (Dự đoán lịch cho activity mới) - MỚI");
        Console.WriteLine("4. Chạy Background Worker (Tự động retrain)");
        Console.WriteLine("5. Test Suggest Schedule for New Activity (Đề xuất lịch cho activity mới)");
        Console.WriteLine("6. Test Constraints (Test các ràng buộc riêng)");
        Console.Write("\nNhập lựa chọn (1-6): ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                await TrainActivityClassificationModelAsync(dbContext);
                break;
            case "2":
                await TrainSchedulePredictionModelAsync(dbContext);
                break;
            case "3":
                await TrainActivityScheduleModelAsync(dbContext);
                break;
            case "4":
                await RunBackgroundWorkerAsync(configuration);
                break;
            case "5":
                await TestSuggestScheduleForNewActivityAsync(dbContext);
                break;
            case "6":
                await TestConstraintsAsync(dbContext);
                break;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ!");
                break;
        }
    }

    /// <summary>
    /// Train model phân loại activity (model cũ)
    /// </summary>
    static async Task TrainActivityClassificationModelAsync(EduShpereDbContext dbContext)
    {
        Console.WriteLine("\n📊 Bắt đầu train Activity Classification Model...");

        var activities = await dbContext.Activities
            .Where(a => !a.IsDeleted && !string.IsNullOrEmpty(a.Description) && !string.IsNullOrEmpty(a.SubType))
            .Select(a => new ActivityData
            {
                Description = a.Description ?? string.Empty,
                Type = a.SubType
            })
            .ToListAsync();

        Console.WriteLine($"✅ Đã đọc {activities.Count} hoạt động từ database");

        if (activities.Count == 0)
        {
            Console.WriteLine("⚠️ Không có dữ liệu trong database.");
            return;
        }

        // Tiền xử lý
        var processedActivities = activities
            .Select(r => new ActivityData
            {
                Description = PreprocessVietnamese(r.Description),
                Type = r.Type
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Description))
            .ToList();

        // Train model - Sử dụng API của ML.NET 2.0.1
        var mlContext = new MLContext();
        var data = mlContext.Data.LoadFromEnumerable(processedActivities);

        // Pipeline với API ML.NET 2.0.1
        var pipeline = mlContext.Transforms.Text
            .FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(ActivityData.Description))
            .Append(mlContext.Transforms.Conversion
                .MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(ActivityData.Type)))
            .Append(mlContext.MulticlassClassification.Trainers
                .SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
            .Append(mlContext.Transforms.Conversion
                .MapKeyToValue(outputColumnName: "PredictedLabel", inputColumnName: "Label"));

        Console.WriteLine("🤖 Đang train model...");
        var model = pipeline.Fit(data);
        Console.WriteLine("✅ Model đã được train thành công!");

        // Lưu model
        string outputPath = Path.Combine("Data", "activity_model.zip");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? "Data");
        mlContext.Model.Save(model, data.Schema, outputPath);
        Console.WriteLine($"✅ Model đã được lưu tại: {Path.GetFullPath(outputPath)}\n");
    }

    /// <summary>
    /// Train model dự đoán schedule cho user (model cũ - giữ lại để tương thích)
    /// </summary>
    static async Task TrainSchedulePredictionModelAsync(EduShpereDbContext dbContext)
    {
        Console.WriteLine("\n📊 Bắt đầu train Schedule Prediction Model (cho user)...");

        var mlService = new ScheduleMLService();
        await mlService.TrainModelAsync(dbContext);

        Console.WriteLine("✅ Schedule Prediction Model đã được train!\n");
    }

    /// <summary>
    /// Train model dự đoán lịch cho activity mới (model mới)
    /// </summary>
    static async Task TrainActivityScheduleModelAsync(EduShpereDbContext dbContext)
    {
        Console.WriteLine("\n📊 Bắt đầu train Activity Schedule Model (cho activity mới)...");

        var mlService = new ActivityScheduleMLService();
        await mlService.TrainModelAsync(dbContext);

        Console.WriteLine("✅ Activity Schedule Model đã được train!\n");
    }

    /// <summary>
    /// Test đề xuất lịch cho activity mới
    /// </summary>
    static async Task TestSuggestScheduleForNewActivityAsync(EduShpereDbContext dbContext)
    {
        Console.WriteLine("\n🧪 Test Suggest Schedule for New Activity...");

        var mlService = new ActivityScheduleMLService();
        mlService.LoadModel();

        var scheduler = new ORToolsScheduler();
        var scheduleService = new ActivityScheduleService(mlService, scheduler, dbContext);

        // Test với activity mới
        Console.WriteLine("\n📝 Thông tin activity mới:");
        Console.WriteLine("   Category: Activity");
        Console.WriteLine("   SubType: SportsFestival");
        Console.WriteLine("   Location: Sân thể thao");
        Console.WriteLine("   Max Participants: 100");

        var preferredStartDate = DateTime.Now.AddDays(7);
        var preferredEndDate = preferredStartDate.AddDays(14);

        Console.WriteLine($"\n📅 Khoảng thời gian đề xuất:");
        Console.WriteLine($"   Từ: {preferredStartDate:dd/MM/yyyy}");
        Console.WriteLine($"   Đến: {preferredEndDate:dd/MM/yyyy}");

        try
        {
            // Đề xuất 1 slot tốt nhất
            var result = await scheduleService.SuggestScheduleForNewActivityAsync(
                ActivityType.Activity,
                "SportsFestival",
                "Sân thể thao",
                100,
                preferredStartDate,
                preferredEndDate,
                TimeSpan.FromHours(3)); // 3 giờ

            if (result.IsOptimal && result.SelectedSlots.Any())
            {
                var bestSlot = result.SelectedSlots.First();
                Console.WriteLine($"\n✅ Đã tìm được lịch đề xuất!");
                Console.WriteLine($"\n📋 Lịch được đề xuất:");
                Console.WriteLine($"   • Thời gian: {bestSlot.StartTime:dd/MM/yyyy HH:mm} - {bestSlot.EndTime:HH:mm}");
                Console.WriteLine($"   • Địa điểm: {bestSlot.Location ?? "N/A"}");
                Console.WriteLine($"   • Điểm AI: {bestSlot.MLScore:P0} (dự đoán thành công)");

                if (!string.IsNullOrEmpty(result.Explanation))
                {
                    Console.WriteLine($"\n💡 Giải thích:");
                    Console.WriteLine(result.Explanation);
                }
            }
            else
            {
                Console.WriteLine($"\n⚠️ {result.Explanation}");
            }

            // Đề xuất nhiều lựa chọn
            Console.WriteLine($"\n📋 Đề xuất {5} lựa chọn khác:");
            var options = await scheduleService.SuggestMultipleScheduleOptionsAsync(
                ActivityType.Activity,
                "SportsFestival",
                "Sân thể thao",
                100,
                preferredStartDate,
                preferredEndDate,
                TimeSpan.FromHours(3),
                numberOfOptions: 5);

            if (options.Any())
            {
                var optionsCount = options.Count;
                for (int i = 0; i < optionsCount; i++)
                {
                    var option = options[i];
                    Console.WriteLine($"\n   Option {i + 1}:");
                    Console.WriteLine($"   • {option.StartTime:dd/MM/yyyy HH:mm} - {option.EndTime:HH:mm}");
                    Console.WriteLine($"   • Điểm AI: {option.MLScore:P0}");
                    Console.WriteLine($"   • Địa điểm: {option.Location ?? "N/A"}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Test các ràng buộc riêng
    /// </summary>
    static async Task TestConstraintsAsync(EduShpereDbContext dbContext)
    {
        Console.WriteLine("\n🧪 Test Constraints (Ràng buộc)...");

        var scheduler = new ORToolsScheduler();

        // Tạo test slots
        var testSlots = new List<ORToolsScheduler.TimeSlot>
        {
            new ORToolsScheduler.TimeSlot
            {
                Id = 1,
                StartTime = DateTime.Now.AddDays(1).Date.AddHours(9),
                EndTime = DateTime.Now.AddDays(1).Date.AddHours(11),
                ActivityId = 1,
                MLScore = 0.9f,
                IsAvailable = true,
                Location = "Phòng A",
                CurrentParticipants = 10,
                MaxParticipants = 50
            },
            new ORToolsScheduler.TimeSlot
            {
                Id = 2,
                StartTime = DateTime.Now.AddDays(1).Date.AddHours(10),
                EndTime = DateTime.Now.AddDays(1).Date.AddHours(12),
                ActivityId = 2,
                MLScore = 0.8f,
                IsAvailable = true,
                Location = "Phòng A", // Cùng địa điểm với slot 1
                CurrentParticipants = 5,
                MaxParticipants = 30
            },
            new ORToolsScheduler.TimeSlot
            {
                Id = 3,
                StartTime = DateTime.Now.AddDays(1).Date.AddHours(14),
                EndTime = DateTime.Now.AddDays(1).Date.AddHours(16),
                ActivityId = 3,
                MLScore = 0.85f,
                IsAvailable = true,
                Location = "Phòng B",
                CurrentParticipants = 20,
                MaxParticipants = 100
            },
            new ORToolsScheduler.TimeSlot
            {
                Id = 4,
                StartTime = DateTime.Now.AddDays(1).Date.AddHours(7), // Ngoài giờ làm việc
                EndTime = DateTime.Now.AddDays(1).Date.AddHours(8),
                ActivityId = 4,
                MLScore = 0.7f,
                IsAvailable = true,
                Location = "Phòng C",
                CurrentParticipants = 0,
                MaxParticipants = 20
            }
        };

        // Tạo constraints
        var constraints = new List<ORToolsScheduler.ScheduleConstraint>
        {
            new ORToolsScheduler.ScheduleConstraint
            {
                UserId = 1,
                ConflictingActivityIds = new List<int>(), // Không có conflict
                MinGapBetweenActivities = TimeSpan.FromMinutes(30),
                WorkingHoursStart = TimeSpan.FromHours(8),
                WorkingHoursEnd = TimeSpan.FromHours(20),
                ExcludedLocations = new List<string> { "Phòng C" },
                ActivityMaxParticipants = new Dictionary<int, int>
                {
                    { 1, 50 },
                    { 2, 30 },
                    { 3, 100 },
                    { 4, 20 }
                }
            }
        };

        Console.WriteLine("\n📋 Test Slots:");
        foreach (var slot in testSlots)
        {
            Console.WriteLine($"   Slot {slot.Id}: {slot.StartTime:HH:mm} - {slot.EndTime:HH:mm} tại {slot.Location} (Score: {slot.MLScore:P0})");
        }

        Console.WriteLine("\n📋 Constraints:");
        Console.WriteLine($"   - Min Gap: {constraints[0].MinGapBetweenActivities?.TotalMinutes} phút");
        Console.WriteLine($"   - Working Hours: {constraints[0].WorkingHoursStart?.Hours}h - {constraints[0].WorkingHoursEnd?.Hours}h");
        Console.WriteLine($"   - Excluded Locations: {string.Join(", ", constraints[0].ExcludedLocations ?? new List<string>())}");

        try
        {
            var result = scheduler.OptimizeSchedule(testSlots, constraints, maxActivitiesPerDay: 3);

            if (result.IsOptimal)
            {
                Console.WriteLine($"\n✅ Tìm được lịch tối ưu!");
                Console.WriteLine($"   Số slot được chọn: {result.SelectedSlots.Count}");
                Console.WriteLine($"   Objective Value: {result.ObjectiveValue:F2}");

                Console.WriteLine("\n📋 Các slot được chọn:");
                foreach (var slot in result.SelectedSlots)
                {
                    Console.WriteLine($"   • Slot {slot.Id}: {slot.StartTime:HH:mm} - {slot.EndTime:HH:mm}");
                    Console.WriteLine($"     Location: {slot.Location}");
                    Console.WriteLine($"     ML Score: {slot.MLScore:P0}");
                }

                Console.WriteLine("\n💡 Giải thích:");
                Console.WriteLine($"   {result.Explanation}");

                // Giải thích tại sao một số slot không được chọn
                var notSelected = testSlots.Where(s => !result.SelectedSlots.Any(rs => rs.Id == s.Id)).ToList();
                if (notSelected.Any())
                {
                    Console.WriteLine("\n❌ Các slot không được chọn và lý do:");
                    foreach (var slot in notSelected)
                    {
                        var reasons = new List<string>();
                        if (slot.StartTime.TimeOfDay < constraints[0].WorkingHoursStart)
                            reasons.Add("Ngoài giờ làm việc (trước 8h)");
                        if (slot.StartTime.TimeOfDay > constraints[0].WorkingHoursEnd)
                            reasons.Add("Ngoài giờ làm việc (sau 20h)");
                        if (constraints[0].ExcludedLocations?.Contains(slot.Location) == true)
                            reasons.Add("Địa điểm bị loại trừ");
                        if (slot.StartTime < testSlots[0].EndTime && slot.StartTime > testSlots[0].StartTime && slot.Location == testSlots[0].Location)
                            reasons.Add("Conflict thời gian và địa điểm với slot khác");

                        Console.WriteLine($"   • Slot {slot.Id}: {string.Join(", ", reasons)}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"\n⚠️ {result.Explanation}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// [DEPRECATED] Function cũ - đã được thay thế bằng TestSuggestScheduleForNewActivityAsync
    /// </summary>
    static async Task AutoGenerateScheduleAsync_DEPRECATED(EduShpereDbContext dbContext)
    {
        Console.WriteLine("\n🤖 Auto Generate Schedule - Tự động tạo lịch thông minh...");

        var mlService = new ScheduleMLService();
        mlService.LoadModel();

        var scheduler = new ORToolsScheduler();
        var scheduleService = new ScheduleService(mlService, scheduler, dbContext);

        // Test với user ID = 1
        var testUserId = 1;
        var startDate = DateTime.Now.AddDays(1);
        var endDate = startDate.AddDays(14); // 2 tuần

        Console.WriteLine($"\n📅 Tự động tạo lịch cho User {testUserId}");
        Console.WriteLine($"   Từ: {startDate:dd/MM/yyyy}");
        Console.WriteLine($"   Đến: {endDate:dd/MM/yyyy}");
        Console.WriteLine($"   Hệ thống sẽ tự động chọn activities phù hợp nhất dựa trên:");
        Console.WriteLine($"   - Lịch sử tham gia của bạn");
        Console.WriteLine($"   - AI dự đoán sở thích");
        Console.WriteLine($"   - Các ràng buộc về thời gian, địa điểm");

        try
        {
            var result = await scheduleService.AutoGenerateScheduleAsync(
                testUserId,
                startDate,
                endDate,
                maxActivitiesToSuggest: 10);

            if (result.IsOptimal)
            {
                Console.WriteLine($"\n✅ Đã tạo lịch tự động thành công!");
                Console.WriteLine($"   Số slot được đề xuất: {result.SelectedSlots.Count}");
                Console.WriteLine($"   Objective Value: {result.ObjectiveValue:F2}");

                Console.WriteLine("\n📋 Lịch được đề xuất:");
                var slotsByDate = result.SelectedSlots.GroupBy(s => s.StartTime.Date).OrderBy(g => g.Key);
                
                foreach (var dayGroup in slotsByDate)
                {
                    var dayStr = dayGroup.Key.ToString("dddd, dd/MM/yyyy", new System.Globalization.CultureInfo("vi-VN"));
                    Console.WriteLine($"\n   📅 {dayStr}:");
                    
                    foreach (var slot in dayGroup.OrderBy(s => s.StartTime))
                    {
                        var activity = await dbContext.Activities.FindAsync(slot.ActivityId);
                        Console.WriteLine($"      • {slot.StartTime:HH:mm} - {slot.EndTime:HH:mm}");
                        Console.WriteLine($"        {activity?.Title ?? "N/A"}");
                        Console.WriteLine($"        Địa điểm: {slot.Location ?? "N/A"}");
                        Console.WriteLine($"        ML Score: {slot.MLScore:P0} (AI dự đoán phù hợp)");
                        Console.WriteLine($"        Participants: {slot.CurrentParticipants}/{slot.MaxParticipants}");
                    }
                }

                if (!string.IsNullOrEmpty(result.Explanation))
                {
                    Console.WriteLine($"\n💡 Giải thích AI:");
                    var explanations = result.Explanation.Split('\n');
                    foreach (var exp in explanations)
                    {
                        Console.WriteLine($"   {exp}");
                    }
                }

                Console.WriteLine($"\n✨ Lịch này được tạo tự động dựa trên:");
                Console.WriteLine($"   - Thói quen tham gia hoạt động của bạn");
                Console.WriteLine($"   - AI phân tích và dự đoán");
                Console.WriteLine($"   - Tối ưu hóa để tránh conflict");
            }
            else
            {
                Console.WriteLine($"\n⚠️ {result.Explanation}");
                Console.WriteLine($"\n💡 Gợi ý:");
                Console.WriteLine($"   - Thử mở rộng khoảng thời gian");
                Console.WriteLine($"   - Kiểm tra xem có activities nào đang mở đăng ký không");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi: {ex.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Chạy Background Worker
    /// </summary>
    static async Task RunBackgroundWorkerAsync(IConfiguration configuration)
    {
        Console.WriteLine("\n🔄 Khởi động Background Worker...");

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton(configuration);
        services.AddHostedService<ModelRetrainWorker>();

        var serviceProvider = services.BuildServiceProvider();
        var host = serviceProvider.GetRequiredService<IHost>();

        Console.WriteLine("✅ Background Worker đã khởi động. Nhấn Ctrl+C để dừng.\n");

        await host.RunAsync();
    }

    static string PreprocessVietnamese(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        string text = input.ToLower();
        text = text.Normalize(System.Text.NormalizationForm.FormD);
        var chars = text.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
            != System.Globalization.UnicodeCategory.NonSpacingMark);
        text = new string(chars.ToArray());
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-zA-Z\s]", " ");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

        var stopwords = new HashSet<string>(new[]
        {
            "là","của","và","với","cho","các","những","một","được","trong","vào","khi","tại",
            "đã","có","đến","này","đó","rằng","nên","thì","cũng","sẽ","như","đang","ra"
        });

        var words = text.Split(' ')
            .Where(w => !stopwords.Contains(w) && w.Length > 1);

        return string.Join(" ", words);
    }
}

// Models cho Activity Classification
public class ActivityData
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class ActivityPrediction
{
    [Microsoft.ML.Data.ColumnName("PredictedLabel")]
    public string PredictedType { get; set; } = string.Empty;
}
