using EduShpere.MLTrainer.Services;
using EduShpere.Infrastructure;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using EduShpere.Domain.Enum;

namespace EduShpere.MLTrainer.Services;

/// <summary>
/// Service chính kết hợp ML + OR-Tools để tạo lịch thông minh
/// </summary>
public class ScheduleService
{
    private readonly ScheduleMLService _mlService;
    private readonly ORToolsScheduler _scheduler;
    private readonly EduShpereDbContext _dbContext;

    public ScheduleService(ScheduleMLService mlService, ORToolsScheduler scheduler, EduShpereDbContext dbContext)
    {
        _mlService = mlService;
        _scheduler = scheduler;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Tạo lịch tối ưu cho user sử dụng AI
    /// </summary>
    public async Task<ORToolsScheduler.ScheduleResult> GenerateOptimalScheduleAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        List<int> activityIds)
    {
        // 1. Lấy thông tin user và activities
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User không tồn tại");
        }

        var activities = await _dbContext.Activities
            .Where(a => activityIds.Contains(a.Id) && !a.IsDeleted)
            .ToListAsync();

        // 2. Lấy lịch sử tham gia của user
        var userParticipations = await _dbContext.ActivityParticipants
            .Include(ap => ap.Activity)
            .Where(ap => ap.UserId == userId && ap.Status == ParticipantStatus.Joined)
            .ToListAsync();

        var previousCount = userParticipations.Count(ap =>
            ap.Activity.StartDate.HasValue &&
            ap.Activity.StartDate.Value >= startDate.AddDays(-7));

        var preferredHour = userParticipations
            .Where(ap => ap.Activity.StartDate.HasValue)
            .Select(ap => (float)ap.Activity.StartDate.Value.Hour)
            .DefaultIfEmpty(9)
            .Average();

        // 3. Lấy các activities đã đăng ký (để tránh conflict)
        var existingActivities = await _dbContext.ActivityParticipants
            .Include(ap => ap.Activity)
            .Where(ap => ap.UserId == userId &&
                        ap.Status == ParticipantStatus.Joined &&
                        ap.Activity.StartDate.HasValue &&
                        ap.Activity.StartDate.Value >= startDate &&
                        ap.Activity.StartDate.Value <= endDate)
            .Select(ap => ap.Activity)
            .ToListAsync();

        // 4. Tạo danh sách slots với ML scores
        var availableSlots = new List<ORToolsScheduler.TimeSlot>();
        int slotId = 1;

        foreach (var activity in activities)
        {
            var activityType = (int)activity.Category;

            // Dự đoán best slots từ ML
            var predictedSlots = _mlService.PredictBestSlots(
                userId,
                activityType,
                previousCount,
                preferredHour,
                startDate,
                endDate);

            // Tạo TimeSlot objects
            foreach (var (hour, dayOfWeek, score) in predictedSlots.Take(10)) // Top 10 slots
            {
                // Tìm ngày trong khoảng startDate-endDate có dayOfWeek này
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    if ((int)date.DayOfWeek == dayOfWeek)
                    {
                        var slotStart = date.AddHours(hour);
                        var slotEnd = slotStart.AddHours(2); // Giả sử mỗi activity 2 giờ

                        // Kiểm tra conflict với existing activities
                        var hasConflict = existingActivities.Any(ea =>
                            ea.StartDate.HasValue && ea.EndDate.HasValue &&
                            slotStart < ea.EndDate.Value && slotEnd > ea.StartDate.Value);

                        // Đếm số participants hiện tại
                        var currentParticipants = await _dbContext.ActivityParticipants
                            .CountAsync(ap => ap.ActivityId == activity.Id && 
                                            ap.Status == ParticipantStatus.Joined);

                        availableSlots.Add(new ORToolsScheduler.TimeSlot
                        {
                            Id = slotId++,
                            StartTime = slotStart,
                            EndTime = slotEnd,
                            ActivityId = activity.Id,
                            MLScore = score,
                            IsAvailable = !hasConflict && currentParticipants < activity.MaxParticipants,
                            Location = activity.Location,
                            CurrentParticipants = currentParticipants,
                            MaxParticipants = activity.MaxParticipants
                        });
                    }
                }
            }
        }

        // 5. Tạo constraints với đầy đủ ràng buộc
        var constraints = new List<ORToolsScheduler.ScheduleConstraint>
        {
            new ORToolsScheduler.ScheduleConstraint
            {
                UserId = userId,
                ConflictingActivityIds = existingActivities.Select(a => a.Id).ToList(),
                MinGapBetweenActivities = TimeSpan.FromMinutes(30), // Tối thiểu 30 phút giữa các activity
                WorkingHoursStart = TimeSpan.FromHours(8), // Giờ làm việc: 8h
                WorkingHoursEnd = TimeSpan.FromHours(20), // Đến 20h
                ActivityMaxParticipants = activities.ToDictionary(a => a.Id, a => a.MaxParticipants),
                ActivityRegisterDeadline = activities
                    .Where(a => a.EndRegisterDate < DateTime.Now.AddDays(30))
                    .ToDictionary(a => a.Id, a => a.EndRegisterDate)
            }
        };

        // 6. Tối ưu hóa với OR-Tools
        var result = _scheduler.OptimizeSchedule(availableSlots, constraints);

        return result;
    }

    /// <summary>
    /// Gợi ý slot tốt nhất cho một activity
    /// </summary>
    public async Task<ORToolsScheduler.TimeSlot?> SuggestBestSlotAsync(
        int userId,
        int activityId)
    {
        var activity = await _dbContext.Activities.FindAsync(activityId);
        if (activity == null) return null;

        var startDate = DateTime.Now;
        var endDate = startDate.AddDays(30); // Gợi ý trong 30 ngày tới

        var userParticipations = await _dbContext.ActivityParticipants
            .Include(ap => ap.Activity)
            .Where(ap => ap.UserId == userId && ap.Status == ParticipantStatus.Joined)
            .ToListAsync();

        var previousCount = userParticipations.Count(ap =>
            ap.Activity.StartDate.HasValue &&
            ap.Activity.StartDate.Value >= startDate.AddDays(-7));

        var preferredHour = userParticipations
            .Where(ap => ap.Activity.StartDate.HasValue)
            .Select(ap => (float)ap.Activity.StartDate.Value.Hour)
            .DefaultIfEmpty(9)
            .Average();

        var activityType = (int)activity.Category;

        // Dự đoán best slots
        var predictedSlots = _mlService.PredictBestSlots(
            userId,
            activityType,
            previousCount,
            preferredHour,
            startDate,
            endDate);

        if (predictedSlots.Count == 0) return null;

        // Lấy slot tốt nhất
        var bestSlot = predictedSlots.First();
        var bestDate = startDate.Date;
        while ((int)bestDate.DayOfWeek != bestSlot.DayOfWeek && bestDate <= endDate)
        {
            bestDate = bestDate.AddDays(1);
        }

        return new ORToolsScheduler.TimeSlot
        {
            Id = 1,
            StartTime = bestDate.AddHours(bestSlot.Hour),
            EndTime = bestDate.AddHours(bestSlot.Hour + 2),
            ActivityId = activityId,
            MLScore = bestSlot.Score,
            IsAvailable = true
        };
    }

    /// <summary>
    /// Tự động generate lịch từ tất cả activities có sẵn (không cần user chọn)
    /// </summary>
    public async Task<ORToolsScheduler.ScheduleResult> AutoGenerateScheduleAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        int maxActivitiesToSuggest = 10)
    {
        // 1. Lấy tất cả activities có sẵn (không cần StartDate cụ thể, chỉ cần chưa hết hạn đăng ký)
        var availableActivities = await _dbContext.Activities
            .Where(a => !a.IsDeleted && 
                       a.EndRegisterDate >= DateTime.Now) // Chưa hết hạn đăng ký
            .OrderByDescending(a => a.CreatedAt)
            .Take(maxActivitiesToSuggest * 3) // Lấy nhiều hơn để có lựa chọn
            .ToListAsync();

        if (availableActivities.Count == 0)
        {
            return new ORToolsScheduler.ScheduleResult
            {
                IsOptimal = false,
                Explanation = "Không có activity nào phù hợp trong khoảng thời gian này."
            };
        }

        // 2. Lấy lịch sử tham gia của user
        var userParticipations = await _dbContext.ActivityParticipants
            .Include(ap => ap.Activity)
            .Where(ap => ap.UserId == userId && ap.Status == ParticipantStatus.Joined)
            .ToListAsync();

        var previousCount = userParticipations.Count(ap =>
            ap.Activity.StartDate.HasValue &&
            ap.Activity.StartDate.Value >= startDate.AddDays(-7));

        var preferredHour = userParticipations
            .Where(ap => ap.Activity.StartDate.HasValue)
            .Select(ap => (float)ap.Activity.StartDate.Value.Hour)
            .DefaultIfEmpty(9)
            .Average();

        // 3. Lấy các activities đã đăng ký (để tránh conflict)
        var existingActivities = await _dbContext.ActivityParticipants
            .Include(ap => ap.Activity)
            .Where(ap => ap.UserId == userId &&
                        ap.Status == ParticipantStatus.Joined &&
                        ap.Activity.StartDate.HasValue &&
                        ap.Activity.StartDate.Value >= startDate &&
                        ap.Activity.StartDate.Value <= endDate)
            .Select(ap => ap.Activity)
            .ToListAsync();

        // 4. Tính ML score cho mỗi activity và tạo slots
        var allSlots = new List<ORToolsScheduler.TimeSlot>();
        int slotId = 1;

        foreach (var activity in availableActivities)
        {
            var activityType = (int)activity.Category;

            // Dự đoán best slots từ ML (không dùng StartDate thực tế)
            var predictedSlots = _mlService.PredictBestSlots(
                userId,
                activityType,
                previousCount,
                preferredHour,
                startDate,
                endDate);

            // Đếm số participants hiện tại
            var currentParticipants = await _dbContext.ActivityParticipants
                .CountAsync(ap => ap.ActivityId == activity.Id && 
                                ap.Status == ParticipantStatus.Joined);

            // Tạo TimeSlot objects từ ML predictions
            foreach (var (hour, dayOfWeek, score) in predictedSlots.Take(5)) // Top 5 slots cho mỗi activity
            {
                // Tìm ngày trong khoảng startDate-endDate có dayOfWeek này
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    if ((int)date.DayOfWeek == dayOfWeek)
                    {
                        var slotStart = date.AddHours(hour);
                        var slotEnd = slotStart.AddHours(2); // Giả sử mỗi activity 2 giờ

                        // Kiểm tra conflict với existing activities
                        var hasConflict = existingActivities.Any(ea =>
                            ea.StartDate.HasValue && ea.EndDate.HasValue &&
                            slotStart < ea.EndDate.Value && slotEnd > ea.StartDate.Value);

                        // Điều chỉnh score nếu đã đầy hoặc gần đầy
                        var adjustedScore = score;
                        if (currentParticipants >= activity.MaxParticipants)
                        {
                            adjustedScore *= 0.1f; // Giảm score nếu đã đầy
                        }
                        else if (currentParticipants >= activity.MaxParticipants * 0.9f)
                        {
                            adjustedScore *= 0.5f; // Giảm score nếu gần đầy
                        }

                        allSlots.Add(new ORToolsScheduler.TimeSlot
                        {
                            Id = slotId++,
                            StartTime = slotStart,
                            EndTime = slotEnd,
                            ActivityId = activity.Id,
                            MLScore = adjustedScore,
                            IsAvailable = !hasConflict && currentParticipants < activity.MaxParticipants,
                            Location = activity.Location,
                            CurrentParticipants = currentParticipants,
                            MaxParticipants = activity.MaxParticipants
                        });
                    }
                }
            }
        }

        if (allSlots.Count == 0)
        {
            return new ORToolsScheduler.ScheduleResult
            {
                IsOptimal = false,
                Explanation = "Không tìm được slot phù hợp với thói quen của bạn."
            };
        }

        // 5. Tạo constraints
        var constraints = new List<ORToolsScheduler.ScheduleConstraint>
        {
            new ORToolsScheduler.ScheduleConstraint
            {
                UserId = userId,
                ConflictingActivityIds = existingActivities.Select(a => a.Id).ToList(),
                MinGapBetweenActivities = TimeSpan.FromMinutes(30),
                WorkingHoursStart = TimeSpan.FromHours(8),
                WorkingHoursEnd = TimeSpan.FromHours(20),
                ActivityMaxParticipants = availableActivities.ToDictionary(a => a.Id, a => a.MaxParticipants),
                ActivityRegisterDeadline = availableActivities
                    .Where(a => a.EndRegisterDate < DateTime.Now.AddDays(30))
                    .ToDictionary(a => a.Id, a => a.EndRegisterDate)
            }
        };

        // 6. Tối ưu hóa với OR-Tools
        var result = _scheduler.OptimizeSchedule(allSlots, constraints, maxActivitiesPerDay: 3);

        return result;
    }
}

