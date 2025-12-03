using System;
using System.Linq;
using ESP.AIService.Models;
using ESP.AIService.Entities;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ESP.AIService.Services;

public class TournamentScheduleService
{
    private readonly TournamentScheduleMLService _mlService;
    private readonly ORToolsScheduler _scheduler;

    public TournamentScheduleService()
    {
        _mlService = new TournamentScheduleMLService();
        _scheduler = new ORToolsScheduler();
    }

    /// <summary>
    /// Tạo lịch thi đấu cho hội thao
    /// </summary>
    public TournamentScheduleResponse GenerateSchedule(
        TournamentScheduleRequest request,
        EduShpereDbContext dbContext)
    {
        Console.WriteLine($"🎯 Bắt đầu tạo lịch thi đấu cho Activity {request.ActivityId}, Sport {request.SportId}");
        Console.WriteLine($"   Số teams: {request.ClassGroupIds.Count}");
        Console.WriteLine($"   Format: {request.TournamentFormat}");
        Console.WriteLine($"   Thời gian: {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}");

        // Bước 1: Generate slots với ML scores
        Console.WriteLine("📊 Đang tạo slots với ML scores...");
        var slots = _mlService.GenerateSlotsWithScores(request, dbContext);
        Console.WriteLine($"   Đã tạo {slots.Count} slots");

        if (!slots.Any())
        {
            return new TournamentScheduleResponse
            {
                Success = false,
                Explanation = "Không thể tạo slots. Vui lòng kiểm tra lại thời gian và địa điểm."
            };
        }

        // Bước 2: Optimize với OR-Tools
        Console.WriteLine("⚙️ Đang tối ưu hóa lịch với OR-Tools...");
        var response = _scheduler.OptimizeSchedule(request, slots, dbContext);

        if (response.Success)
        {
            Console.WriteLine($"✅ Đã tạo thành công {response.TotalMatches} matches trong {response.TotalRounds} rounds");
            Console.WriteLine($"   Objective value: {response.ObjectiveValue:F2}");
        }
        else
        {
            Console.WriteLine($"❌ Không thể tạo lịch: {response.Explanation}");
        }

        return response;
    }

    /// <summary>
    /// Train ML model từ dữ liệu matches đã có
    /// </summary>
    public void TrainModel(EduShpereDbContext dbContext)
    {
        Console.WriteLine("🔄 Bắt đầu train ML model...");
        _mlService.TrainModel(dbContext);
        Console.WriteLine("✅ Hoàn thành train model");
    }
}

