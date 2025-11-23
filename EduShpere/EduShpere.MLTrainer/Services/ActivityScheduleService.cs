using EduShpere.MLTrainer.Services;
using EduShpere.Infrastructure;
using EduShpere.Domain.Models;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.MLTrainer.Services;

/// <summary>
/// Service để đề xuất lịch cho activity mới
/// </summary>
public class ActivityScheduleService
{
    private readonly ActivityScheduleMLService _mlService;
    private readonly ORToolsScheduler _scheduler;
    private readonly EduShpereDbContext _dbContext;

    public ActivityScheduleService(
        ActivityScheduleMLService mlService,
        ORToolsScheduler scheduler,
        EduShpereDbContext dbContext)
    {
        _mlService = mlService;
        _scheduler = scheduler;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Đề xuất lịch cho activity mới dựa trên AI + OR-Tools
    /// </summary>
    public async Task<ORToolsScheduler.ScheduleResult> SuggestScheduleForNewActivityAsync(
        ActivityType activityType,
        string subType,
        string? location,
        int maxParticipants,
        DateTime preferredStartDate,
        DateTime preferredEndDate,
        TimeSpan? duration = null)
    {
        duration ??= TimeSpan.FromHours(2); // Mặc định 2 giờ

        // 1. Dự đoán các slot tốt nhất từ ML
        var predictedSlots = _mlService.PredictBestTimeSlots(
            activityType,
            subType,
            location,
            maxParticipants,
            preferredStartDate,
            preferredEndDate);

        // 2. Lấy tất cả activities đã có trong khoảng thời gian (để tránh conflict)
        var existingActivities = await _dbContext.Activities
            .Where(a => !a.IsDeleted &&
                       a.StartDate.HasValue &&
                       a.EndDate.HasValue &&
                       a.StartDate.Value >= preferredStartDate.AddDays(-7) &&
                       a.StartDate.Value <= preferredEndDate.AddDays(7))
            .ToListAsync();

        // 3. Tạo TimeSlot objects
        var availableSlots = new List<ORToolsScheduler.TimeSlot>();
        int slotId = 1;

        foreach (var (startTime, score) in predictedSlots.Take(50)) // Top 50 slots
        {
            var endTime = startTime.Add(duration.Value);

            // Kiểm tra conflict với existing activities
            var hasConflict = existingActivities.Any(ea =>
                ea.StartDate.HasValue && ea.EndDate.HasValue &&
                startTime < ea.EndDate.Value && endTime > ea.StartDate.Value);

            // Kiểm tra location conflict
            var hasLocationConflict = !string.IsNullOrEmpty(location) &&
                existingActivities.Any(ea =>
                    ea.Location == location &&
                    ea.StartDate.HasValue && ea.EndDate.HasValue &&
                    startTime < ea.EndDate.Value && endTime > ea.StartDate.Value);

            // Điều chỉnh score nếu có conflict
            var adjustedScore = score;
            if (hasConflict)
            {
                adjustedScore *= 0.3f; // Giảm mạnh nếu conflict
            }
            if (hasLocationConflict)
            {
                adjustedScore *= 0.2f; // Giảm rất mạnh nếu location conflict
            }

            availableSlots.Add(new ORToolsScheduler.TimeSlot
            {
                Id = slotId++,
                StartTime = startTime,
                EndTime = endTime,
                ActivityId = 0, // Chưa có activity ID (activity mới)
                MLScore = adjustedScore,
                IsAvailable = !hasConflict && !hasLocationConflict,
                Location = location,
                CurrentParticipants = 0,
                MaxParticipants = maxParticipants
            });
        }

        if (availableSlots.Count == 0)
        {
            return new ORToolsScheduler.ScheduleResult
            {
                IsOptimal = false,
                Explanation = "Không tìm được slot phù hợp. Vui lòng thử khoảng thời gian khác."
            };
        }

        // 4. Tạo constraints
        var constraints = new List<ORToolsScheduler.ScheduleConstraint>
        {
            new ORToolsScheduler.ScheduleConstraint
            {
                UserId = 0, // Không phải cho user cụ thể
                ConflictingActivityIds = existingActivities.Select(a => a.Id).ToList(),
                MinGapBetweenActivities = TimeSpan.FromMinutes(30),
                WorkingHoursStart = TimeSpan.FromHours(8),
                WorkingHoursEnd = TimeSpan.FromHours(20),
                ExcludedLocations = new List<string>() // Có thể thêm nếu cần
            }
        };

        // 5. Tối ưu hóa với OR-Tools (chọn 1 slot tốt nhất)
        var result = _scheduler.OptimizeSchedule(availableSlots, constraints, maxActivitiesPerDay: 1);

        // 6. Tạo explanation chi tiết hơn
        if (result.IsOptimal && result.SelectedSlots.Any())
        {
            var bestSlot = result.SelectedSlots.First();
            var explanation = new System.Text.StringBuilder();
            explanation.AppendLine($"Slot được đề xuất: {bestSlot.StartTime:dd/MM/yyyy HH:mm} - {bestSlot.EndTime:HH:mm}");
            explanation.AppendLine($"Điểm AI dự đoán: {(int)(bestSlot.MLScore * 100)}% phù hợp");
            explanation.AppendLine($"Lý do:");
            explanation.AppendLine($"- Dựa trên pattern của {existingActivities.Count} activities tương tự");
            explanation.AppendLine($"- Loại activity: {activityType}, SubType: {subType}");
            if (!string.IsNullOrEmpty(location))
            {
                explanation.AppendLine($"- Địa điểm: {location}");
            }
            explanation.AppendLine($"- Không conflict với các activities đã có");
            explanation.AppendLine($"- Trong giờ làm việc (8h-20h)");

            result.Explanation = explanation.ToString();
        }

        return result;
    }

    /// <summary>
    /// Đề xuất nhiều lựa chọn lịch cho activity mới
    /// </summary>
    public async Task<List<ORToolsScheduler.TimeSlot>> SuggestMultipleScheduleOptionsAsync(
        ActivityType activityType,
        string subType,
        string? location,
        int maxParticipants,
        DateTime preferredStartDate,
        DateTime preferredEndDate,
        TimeSpan? duration = null,
        int numberOfOptions = 5)
    {
        duration ??= TimeSpan.FromHours(2);

        // Dự đoán các slot tốt nhất
        var predictedSlots = _mlService.PredictBestTimeSlots(
            activityType,
            subType,
            location,
            maxParticipants,
            preferredStartDate,
            preferredEndDate);

        // Lấy activities đã có
        var existingActivities = await _dbContext.Activities
            .Where(a => !a.IsDeleted &&
                       a.StartDate.HasValue &&
                       a.EndDate.HasValue &&
                       a.StartDate.Value >= preferredStartDate.AddDays(-7) &&
                       a.StartDate.Value <= preferredEndDate.AddDays(7))
            .ToListAsync();

        // Tạo slots và filter
        var options = new List<ORToolsScheduler.TimeSlot>();
        int slotId = 1;

        foreach (var (startTime, score) in predictedSlots)
        {
            var endTime = startTime.Add(duration.Value);

            // Kiểm tra conflict
            var hasConflict = existingActivities.Any(ea =>
                ea.StartDate.HasValue && ea.EndDate.HasValue &&
                startTime < ea.EndDate.Value && endTime > ea.StartDate.Value);

            var hasLocationConflict = !string.IsNullOrEmpty(location) &&
                existingActivities.Any(ea =>
                    ea.Location == location &&
                    ea.StartDate.HasValue && ea.EndDate.HasValue &&
                    startTime < ea.EndDate.Value && endTime > ea.StartDate.Value);

            if (!hasConflict && !hasLocationConflict)
            {
                options.Add(new ORToolsScheduler.TimeSlot
                {
                    Id = slotId++,
                    StartTime = startTime,
                    EndTime = endTime,
                    ActivityId = 0,
                    MLScore = score,
                    IsAvailable = true,
                    Location = location,
                    CurrentParticipants = 0,
                    MaxParticipants = maxParticipants
                });

                if (options.Count >= numberOfOptions)
                    break;
            }
        }

        return options.OrderByDescending(o => o.MLScore).ToList();
    }
}

