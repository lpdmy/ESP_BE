using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ESP.AIService.Services;
using ESP.AIService.Models;
using EduShpere.Infrastructure;

namespace ESP.AIService;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==================================================================================");
        Console.WriteLine("           🏆 AI TOURNAMENT SCHEDULE GENERATOR - HỘI THAO 🏆");
        Console.WriteLine("==================================================================================");
        Console.WriteLine();

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnectionString");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ Không tìm thấy connection string trong appsettings.json");
            Console.WriteLine("   Vui lòng cấu hình ConnectionStrings:DefaultConnectionString");
            return;
        }

        // Setup DbContext
        var optionsBuilder = new DbContextOptionsBuilder<EduShpereDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        
        using var dbContext = new EduShpereDbContext(optionsBuilder.Options);
        
        // Test connection
        try
        {
            dbContext.Database.CanConnect();
            Console.WriteLine("✅ Kết nối database thành công!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Không thể kết nối database: {ex.Message}");
            Console.WriteLine("   Vui lòng kiểm tra connection string và đảm bảo database đã được tạo.");
            return;
        }

        var scheduleService = new TournamentScheduleService();

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("==================================================================================");
            Console.WriteLine("MENU:");
            Console.WriteLine("  1. Train ML Model (Train model từ dữ liệu matches đã có)");
            Console.WriteLine("  2. Generate Tournament Schedule (Tạo lịch thi đấu)");
            Console.WriteLine("  3. Exit");
            Console.WriteLine("==================================================================================");
            Console.Write("Chọn option (1-3): ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    TrainModel(scheduleService, dbContext);
                    break;
                case "2":
                    GenerateSchedule(scheduleService, dbContext);
                    break;
                case "3":
                    Console.WriteLine("👋 Tạm biệt!");
                    return;
                default:
                    Console.WriteLine("❌ Lựa chọn không hợp lệ. Vui lòng chọn 1-3.");
                    break;
            }
        }
    }

    static void TrainModel(TournamentScheduleService service, EduShpereDbContext dbContext)
    {
        Console.WriteLine();
        Console.WriteLine("🔄 Bắt đầu train ML model...");
        Console.WriteLine("   Model sẽ học từ các matches đã completed trong database.");
        Console.WriteLine();
        
        service.TrainModel(dbContext);
    }

    static void GenerateSchedule(TournamentScheduleService service, EduShpereDbContext dbContext)
    {
        Console.WriteLine();
        Console.WriteLine("📋 Nhập thông tin để tạo lịch thi đấu:");
        Console.WriteLine();

        try
        {
            // Activity ID
            Console.Write("Activity ID: ");
            var activityIdInput = Console.ReadLine();
            if (!int.TryParse(activityIdInput, out int activityId))
            {
                Console.WriteLine("❌ Activity ID không hợp lệ.");
                return;
            }

            // Sport ID
            Console.Write("Sport ID: ");
            var sportIdInput = Console.ReadLine();
            if (!int.TryParse(sportIdInput, out int sportId))
            {
                Console.WriteLine("❌ Sport ID không hợp lệ.");
                return;
            }

            // Class Group IDs
            Console.Write("Class Group IDs (phân cách bằng dấu phẩy, ví dụ: 1,2,3,4): ");
            var classGroupIdsInput = Console.ReadLine();
            var classGroupIds = classGroupIdsInput?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out int id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList() ?? new List<int>();

            if (classGroupIds.Count < 2)
            {
                Console.WriteLine("❌ Cần ít nhất 2 class groups để tạo lịch thi đấu.");
                return;
            }

            // Grade (optional)
            Console.Write("Grade (Enter để bỏ qua): ");
            var gradeInput = Console.ReadLine();
            int? grade = null;
            if (!string.IsNullOrWhiteSpace(gradeInput) && int.TryParse(gradeInput, out int gradeValue))
            {
                grade = gradeValue;
            }

            // Start Date
            Console.Write("Start Date (dd/MM/yyyy): ");
            var startDateInput = Console.ReadLine();
            if (!DateTime.TryParseExact(startDateInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
            {
                Console.WriteLine("❌ Start Date không hợp lệ. Format: dd/MM/yyyy");
                return;
            }

            // End Date
            Console.Write("End Date (dd/MM/yyyy): ");
            var endDateInput = Console.ReadLine();
            if (!DateTime.TryParseExact(endDateInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
            {
                Console.WriteLine("❌ End Date không hợp lệ. Format: dd/MM/yyyy");
                return;
            }

            // Match Duration (hours)
            Console.Write("Match Duration (giờ, mặc định 1): ");
            var durationInput = Console.ReadLine();
            var durationHours = 1.0;
            if (!string.IsNullOrWhiteSpace(durationInput) && double.TryParse(durationInput, out double hours))
            {
                durationHours = hours;
            }

            // Preferred Start Time
            Console.Write("Preferred Start Time (HH:mm, mặc định 08:00): ");
            var startTimeInput = Console.ReadLine();
            var startTime = TimeSpan.FromHours(8);
            if (!string.IsNullOrWhiteSpace(startTimeInput) && TimeSpan.TryParse(startTimeInput, out TimeSpan parsedStartTime))
            {
                startTime = parsedStartTime;
            }

            // Preferred End Time
            Console.Write("Preferred End Time (HH:mm, mặc định 17:00): ");
            var endTimeInput = Console.ReadLine();
            var endTime = TimeSpan.FromHours(17);
            if (!string.IsNullOrWhiteSpace(endTimeInput) && TimeSpan.TryParse(endTimeInput, out TimeSpan parsedEndTime))
            {
                endTime = parsedEndTime;
            }

            // Available Locations
            Console.Write("Available Locations (phân cách bằng dấu phẩy, ví dụ: Sân A,Sân B): ");
            var locationsInput = Console.ReadLine();
            var locations = locationsInput?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList() ?? new List<string>();

            // Tournament Format
            Console.Write("Tournament Format (SingleElimination/RoundRobin/DoubleElimination, mặc định SingleElimination): ");
            var formatInput = Console.ReadLine();
            var format = string.IsNullOrWhiteSpace(formatInput) ? "SingleElimination" : formatInput;

            // Max Matches Per Day
            Console.Write("Max Matches Per Day (mặc định 10): ");
            var maxMatchesInput = Console.ReadLine();
            var maxMatches = 10;
            if (!string.IsNullOrWhiteSpace(maxMatchesInput) && int.TryParse(maxMatchesInput, out int max))
            {
                maxMatches = max;
            }

            // Min Gap Between Matches (minutes)
            Console.Write("Min Gap Between Matches (phút, mặc định 30): ");
            var gapInput = Console.ReadLine();
            var gapMinutes = 30;
            if (!string.IsNullOrWhiteSpace(gapInput) && int.TryParse(gapInput, out int gap))
            {
                gapMinutes = gap;
            }

            // Tạo request
            var request = new TournamentScheduleRequest
            {
                ActivityId = activityId,
                SportId = sportId,
                ClassGroupIds = classGroupIds,
                Grade = grade,
                StartDate = startDate,
                EndDate = endDate,
                MatchDuration = TimeSpan.FromHours(durationHours),
                PreferredStartTime = startTime,
                PreferredEndTime = endTime,
                AvailableLocations = locations,
                MaxMatchesPerDay = maxMatches,
                MinGapBetweenMatches = gapMinutes,
                TournamentFormat = format
            };

            // Generate schedule
            Console.WriteLine();
            var response = service.GenerateSchedule(request, dbContext);

            // Hiển thị kết quả
            Console.WriteLine();
            Console.WriteLine("==================================================================================");
            Console.WriteLine("KẾT QUẢ:");
            Console.WriteLine($"  Success: {response.Success}");
            Console.WriteLine($"  Is Optimal: {response.IsOptimal}");
            Console.WriteLine($"  Total Matches: {response.TotalMatches}");
            Console.WriteLine($"  Total Rounds: {response.TotalRounds}");
            Console.WriteLine($"  Objective Value: {response.ObjectiveValue:F2}");
            Console.WriteLine($"  Explanation: {response.Explanation}");
            Console.WriteLine();

            if (response.Success && response.GeneratedMatches.Any())
            {
                Console.WriteLine("CHI TIẾT CÁC MATCHES:");
                Console.WriteLine("--------------------------------------------------------------------------------");
                foreach (var match in response.GeneratedMatches)
                {
                    Console.WriteLine($"  Match #{match.MatchNumber} - Round {match.Round} ({match.RoundName})");
                    
                    // Hiển thị teams (có thể null nếu chưa biết)
                    if (match.ClassGroup1Id.HasValue && match.ClassGroup2Id.HasValue)
                    {
                        Console.WriteLine($"    Team 1: ClassGroup {match.ClassGroup1Id}");
                        Console.WriteLine($"    Team 2: ClassGroup {match.ClassGroup2Id}");
                    }
                    else
                    {
                        Console.WriteLine($"    Team 1: {match.ClassGroup1Id?.ToString() ?? "Chờ kết quả vòng trước"}");
                        Console.WriteLine($"    Team 2: {match.ClassGroup2Id?.ToString() ?? "Chờ kết quả vòng trước"}");
                        if (!string.IsNullOrEmpty(match.Notes))
                        {
                            Console.WriteLine($"    📝 {match.Notes}");
                        }
                    }
                    
                    Console.WriteLine($"    Date: {match.MatchDate:dd/MM/yyyy}");
                    Console.WriteLine($"    Time: {match.StartTime:hh\\:mm} - {match.EndTime:hh\\:mm}");
                    Console.WriteLine($"    Location: {match.Location ?? "N/A"}");
                    Console.WriteLine($"    Status: {match.Status}");
                    
                    // Chỉ hiển thị kết quả nếu match đã completed
                    if (match.Status == EduShpere.Domain.Enum.MatchStatus.Completed)
                    {
                        if (match.Score1.HasValue && match.Score2.HasValue)
                        {
                            Console.WriteLine($"    Score: {match.Score1} - {match.Score2}");
                        }
                        if (match.WinnerClassGroupId.HasValue)
                        {
                            Console.WriteLine($"    Winner: ClassGroup {match.WinnerClassGroupId}");
                        }
                    }
                    else if (match.Round > 1 && (!match.ClassGroup1Id.HasValue || !match.ClassGroup2Id.HasValue))
                    {
                        Console.WriteLine($"    ⏳ Lịch đã được tạo sẵn - Teams sẽ được xác định sau khi vòng trước hoàn thành");
                    }
                    
                    // Hiển thị NextMatchId nếu có
                    if (match.NextMatchId.HasValue)
                    {
                        Console.WriteLine($"    ➡️  Winner sẽ vào Match #{match.NextMatchId}");
                    }
                    
                    Console.WriteLine();
                }
            }
            Console.WriteLine("==================================================================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Lỗi: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
        }
    }
}
