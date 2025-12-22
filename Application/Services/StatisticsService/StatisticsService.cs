using EduShpere.Application.DTOs.StatisticsDto;
using EduShpere.Domain;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EduShpere.Application.Services.StatisticsService;

public class StatisticsService : IStatisticsService
{
    private readonly EduShpereDbContext _context;

    public StatisticsService(EduShpereDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityOverviewStatisticsDto> GetActivityOverviewStatisticsAsync(int? academicYearId = null)
    {
        var now = DateTime.UtcNow;
        var query = _context.Activities.Where(a => !a.IsDeleted);

        // Filter by academic year if provided
        if (academicYearId.HasValue)
        {
            var academicYear = await _context.AcademicYears.FindAsync(academicYearId.Value);
            if (academicYear != null)
            {
                query = query.Where(a => 
                    (a.StartDate.HasValue && a.StartDate >= academicYear.StartDate && a.StartDate <= academicYear.EndDate) ||
                    (a.EndDate.HasValue && a.EndDate >= academicYear.StartDate && a.EndDate <= academicYear.EndDate) ||
                    (a.StartDate.HasValue && a.EndDate.HasValue && a.StartDate <= academicYear.EndDate && a.EndDate >= academicYear.StartDate)
                );
            }
        }

        var activities = await query
            .Include(a => a.ActivityParticipants)
            .ToListAsync();

        var result = new ActivityOverviewStatisticsDto
        {
            TotalCreated = activities.Count,
            Ongoing = activities.Count(a => a.StartDate.HasValue && a.StartDate <= now && a.EndDate.HasValue && a.EndDate >= now),
            Completed = activities.Count(a => a.EndDate.HasValue && a.EndDate < now),
            Cancelled = activities.Count(a => a.IsDeleted) // Note: IsDeleted might mean cancelled
        };

        // Phân loại theo SubType
        result.ByType = activities
            .GroupBy(a => a.SubType ?? "Unknown")
            .Select(g => new ActivityTypeCountDto
            {
                Type = g.Key,
                Count = g.Count(),
                TotalParticipants = g.SelectMany(a => a.ActivityParticipants).Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // Top sự kiện có nhiều người tham gia nhất
        result.TopActivitiesByParticipants = activities
            .Select(a => new TopActivityDto
            {
                ActivityId = a.Id,
                Title = a.Title ?? "N/A",
                ParticipantCount = a.ActivityParticipants.Count(ap => ap.Status == ParticipantStatus.Joined),
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .OrderByDescending(x => x.ParticipantCount)
            .Take(10)
            .ToList();

        // Phân bổ theo năm học
        var academicYears = await _context.AcademicYears.ToListAsync();
        result.ByAcademicYear = academicYears.Select(ay => new ActivityByAcademicYearDto
        {
            AcademicYearId = ay.Id,
            AcademicYearName = ay.Name,
            ActivityCount = activities.Count(a =>
                (a.StartDate.HasValue && a.StartDate >= ay.StartDate && a.StartDate <= ay.EndDate) ||
                (a.EndDate.HasValue && a.EndDate >= ay.StartDate && a.EndDate <= ay.EndDate) ||
                (a.StartDate.HasValue && a.EndDate.HasValue && a.StartDate <= ay.EndDate && a.EndDate >= ay.StartDate)
            ),
            TotalParticipants = activities
                .Where(a => (a.StartDate.HasValue && a.StartDate >= ay.StartDate && a.StartDate <= ay.EndDate) ||
                           (a.EndDate.HasValue && a.EndDate >= ay.StartDate && a.EndDate <= ay.EndDate) ||
                           (a.StartDate.HasValue && a.EndDate.HasValue && a.StartDate <= ay.EndDate && a.EndDate >= ay.StartDate))
                .SelectMany(a => a.ActivityParticipants)
                .Count(ap => ap.Status == ParticipantStatus.Joined)
        }).ToList();

        // Phân bổ theo tháng (trong năm học hiện tại hoặc năm học được chọn)
        var currentAcademicYear = academicYearId.HasValue
            ? await _context.AcademicYears.FindAsync(academicYearId.Value)
            : await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsCurrent);

        if (currentAcademicYear != null)
        {
            var monthNames = new[] { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
                "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };

            result.ByMonth = Enumerable.Range(1, 12).Select(month => new ActivityByMonthDto
            {
                Month = month,
                MonthName = monthNames[month - 1],
                ActivityCount = activities.Count(a =>
                    a.StartDate.HasValue &&
                    a.StartDate.Value.Year == currentAcademicYear.StartDate.Year &&
                    a.StartDate.Value.Month == month &&
                    a.StartDate >= currentAcademicYear.StartDate &&
                    a.StartDate <= currentAcademicYear.EndDate
                ),
                TotalParticipants = activities
                    .Where(a => a.StartDate.HasValue &&
                               a.StartDate.Value.Year == currentAcademicYear.StartDate.Year &&
                               a.StartDate.Value.Month == month &&
                               a.StartDate >= currentAcademicYear.StartDate &&
                               a.StartDate <= currentAcademicYear.EndDate)
                    .SelectMany(a => a.ActivityParticipants)
                    .Count(ap => ap.Status == ParticipantStatus.Joined)
            }).ToList();
        }

        return result;
    }

    public async Task<ActivityDetailStatisticsDto> GetActivityDetailStatisticsAsync(int activityId)
    {
        var activity = await _context.Activities
            .Include(a => a.ActivityParticipants)
                .ThenInclude(ap => ap.ClassGroup)
            .Include(a => a.ActivityRewards)
            .FirstOrDefaultAsync(a => a.Id == activityId && !a.IsDeleted);

        if (activity == null)
        {
            throw new KeyNotFoundException($"Activity with ID {activityId} not found");
        }

        var participants = activity.ActivityParticipants.ToList();
        var totalRegistered = participants.Count;
        var actualParticipants = participants.Count(p => p.Status == ParticipantStatus.Joined);
        var participationRate = totalRegistered > 0 ? (double)actualParticipants / totalRegistered * 100 : 0;

        // Tính tổng điểm đã trao từ PointHistory (chỉ tính điểm Earn và loại trừ điểm âm)
        var participantUserIds = participants.Where(p => p.Status == ParticipantStatus.Joined).Select(p => p.UserId).ToList();
        var totalPointsAwarded = await _context.PointHistory
            .Where(ph => participantUserIds.Contains(ph.UserId) &&
                        ph.Description != null &&
                        (ph.Description.Contains($"Activity {activityId}") || ph.Description.Contains(activity.Title ?? "")) &&
                        ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0) // Chỉ tính điểm dương
            .SumAsync(ph => ph.Points);

        var averagePointsPerParticipant = actualParticipants > 0 ? (double)totalPointsAwarded / actualParticipants : 0;

        // Số giải thưởng đã trao
        var totalRewardsAwarded = activity.ActivityRewards.Count(r => !r.IsDeleted);

        var result = new ActivityDetailStatisticsDto
        {
            ActivityId = activity.Id,
            ActivityTitle = activity.Title ?? "N/A",
            TotalRegistered = totalRegistered,
            ActualParticipants = actualParticipants,
            ParticipationRate = Math.Round(participationRate, 2),
            TotalPointsAwarded = totalPointsAwarded,
            AveragePointsPerParticipant = Math.Round(averagePointsPerParticipant, 2),
            TotalRewardsAwarded = totalRewardsAwarded
        };

        // Phân bổ theo lớp
        result.ParticipationByClassGroup = participants
            .Where(p => p.ClassGroupId.HasValue)
            .GroupBy(p => new { p.ClassGroupId, p.ClassGroup!.Name })
            .Select(g => new ClassGroupParticipationDto
            {
                ClassGroupId = g.Key.ClassGroupId!.Value,
                ClassGroupName = g.Key.Name ?? "N/A",
                RegisteredCount = g.Count(),
                ActualParticipantsCount = g.Count(p => p.Status == ParticipantStatus.Joined),
                PointsAwarded = 0 // Will be calculated separately if needed
            })
            .ToList();

        // Timeline data (theo ngày đăng ký)
        var registrationDates = participants
            .Where(p => p.CreatedAt.HasValue)
            .GroupBy(p => p.CreatedAt!.Value.Date)
            .OrderBy(g => g.Key)
            .Select(g => new ParticipationTimelineDto
            {
                Date = g.Key,
                RegisteredCount = g.Count(),
                ActualParticipantsCount = g.Count(p => p.Status == ParticipantStatus.Joined)
            })
            .ToList();

        result.ParticipationTimeline = registrationDates;

        return result;
    }

    public async Task<AcademicYearStatisticsDto> GetAcademicYearStatisticsAsync(int academicYearId)
    {
        var academicYear = await _context.AcademicYears.FindAsync(academicYearId);
        if (academicYear == null)
        {
            throw new KeyNotFoundException($"AcademicYear with ID {academicYearId} not found");
        }

        var classes = await _context.ClassGroups
            .Where(cg => cg.AcademicYearId == academicYearId && !cg.IsDeleted)
            .Include(cg => cg.ClassGroupMembers)
            .ToListAsync();

        var students = classes.SelectMany(c => c.ClassGroupMembers).Select(m => m.UserId).Distinct().ToList();
        var totalStudents = students.Count;

        // Lấy các activities trong năm học
        var activities = await _context.Activities
            .Where(a => !a.IsDeleted &&
                       ((a.StartDate.HasValue && a.StartDate >= academicYear.StartDate && a.StartDate <= academicYear.EndDate) ||
                        (a.EndDate.HasValue && a.EndDate >= academicYear.StartDate && a.EndDate <= academicYear.EndDate) ||
                        (a.StartDate.HasValue && a.EndDate.HasValue && a.StartDate <= academicYear.EndDate && a.EndDate >= academicYear.StartDate)))
            .Include(a => a.ActivityParticipants)
            .ToListAsync();

        // Số câu lạc bộ đang hoạt động
        var activeClubs = await _context.Clubs
            .Where(c => !c.IsDeleted &&
                       c.ClubMembers.Any(cm => !cm.IsDeleted))
            .CountAsync();

        var result = new AcademicYearStatisticsDto
        {
            AcademicYearId = academicYear.Id,
            AcademicYearName = academicYear.Name,
            StartDate = academicYear.StartDate,
            EndDate = academicYear.EndDate,
            TotalClasses = classes.Count,
            TotalStudents = totalStudents,
            TotalActivities = activities.Count,
            ActiveClubs = activeClubs
        };

        // Thống kê theo tháng
        var monthNames = new[] { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
            "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };

        result.MonthlyActivities = Enumerable.Range(1, 12).Select(month =>
        {
            var monthActivities = activities.Where(a =>
                a.StartDate.HasValue &&
                a.StartDate.Value.Month == month &&
                a.StartDate >= academicYear.StartDate &&
                a.StartDate <= academicYear.EndDate).ToList();

            var participantIds = monthActivities
                .SelectMany(a => a.ActivityParticipants)
                .Where(ap => ap.Status == ParticipantStatus.Joined)
                .Select(ap => ap.UserId)
                .Distinct()
                .ToList();

            var pointsAwarded = _context.PointHistory
                .Where(ph => participantIds.Contains(ph.UserId) &&
                            ph.CreatedAt.Month == month &&
                            ph.CreatedAt >= academicYear.StartDate &&
                            ph.CreatedAt <= academicYear.EndDate &&
                            ph.ActionType == PointActionType.Earn &&
                            ph.Points > 0) // Chỉ tính điểm dương
                .Sum(ph => ph.Points);

            return new MonthlyActivityDto
            {
                Month = month,
                MonthName = monthNames[month - 1],
                ActivityCount = monthActivities.Count,
                ParticipantCount = participantIds.Count,
                TotalPointsAwarded = pointsAwarded
            };
        }).ToList();

        // Top lớp tích cực nhất
        var classActivityCounts = await _context.ActivityParticipants
            .Where(ap => ap.ClassGroupId.HasValue &&
                        ap.Status == ParticipantStatus.Joined &&
                        ap.Activity != null &&
                        !ap.Activity.IsDeleted &&
                        ((ap.Activity.StartDate.HasValue && ap.Activity.StartDate >= academicYear.StartDate && ap.Activity.StartDate <= academicYear.EndDate) ||
                         (ap.Activity.EndDate.HasValue && ap.Activity.EndDate >= academicYear.StartDate && ap.Activity.EndDate <= academicYear.EndDate)))
            .GroupBy(ap => new { ap.ClassGroupId, ap.ClassGroup!.Name })
            .Select(g => new
            {
                ClassGroupId = g.Key.ClassGroupId!.Value,
                ClassGroupName = g.Key.Name ?? "N/A",
                ActivityCount = g.Select(ap => ap.ActivityId).Distinct().Count(),
                ParticipantCount = g.Count(),
                ParticipantUserIds = g.Select(ap => ap.UserId).Distinct().ToList()
            })
            .ToListAsync();

        result.TopActiveClasses = classActivityCounts
            .Select(cac =>
            {
                var pointsAwarded = _context.PointHistory
                    .Where(ph => cac.ParticipantUserIds.Contains(ph.UserId) &&
                                ph.CreatedAt >= academicYear.StartDate &&
                                ph.CreatedAt <= academicYear.EndDate &&
                                ph.ActionType == PointActionType.Earn &&
                                ph.Points > 0) // Chỉ tính điểm dương
                    .Sum(ph => ph.Points);

                return new TopClassGroupDto
                {
                    ClassGroupId = cac.ClassGroupId,
                    ClassGroupName = cac.ClassGroupName,
                    ActivityCount = cac.ActivityCount,
                    ParticipantCount = cac.ParticipantCount,
                    TotalPointsAwarded = pointsAwarded
                };
            })
            .OrderByDescending(x => x.ActivityCount)
            .Take(10)
            .ToList();

        // Top học sinh tích cực nhất
        var studentActivityCounts = await _context.ActivityParticipants
            .Where(ap => ap.Status == ParticipantStatus.Joined &&
                        ap.Activity != null &&
                        !ap.Activity.IsDeleted &&
                        ((ap.Activity.StartDate.HasValue && ap.Activity.StartDate >= academicYear.StartDate && ap.Activity.StartDate <= academicYear.EndDate) ||
                         (ap.Activity.EndDate.HasValue && ap.Activity.EndDate >= academicYear.StartDate && ap.Activity.EndDate <= academicYear.EndDate)))
            .GroupBy(ap => new { ap.UserId, ap.User!.FirstName, ap.User.LastName, ap.User.AvatarUrl })
            .Select(g => new
            {
                UserId = g.Key.UserId,
                FullName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                AvatarUrl = g.Key.AvatarUrl,
                ActivityCount = g.Select(ap => ap.ActivityId).Distinct().Count()
            })
            .ToListAsync();

        result.TopActiveStudents = studentActivityCounts
            .Select(sac =>
            {
                var pointsAwarded = _context.PointHistory
                    .Where(ph => ph.UserId == sac.UserId &&
                                ph.CreatedAt >= academicYear.StartDate &&
                                ph.CreatedAt <= academicYear.EndDate &&
                                ph.ActionType == PointActionType.Earn &&
                                ph.Points > 0) // Chỉ tính điểm dương
                    .Sum(ph => ph.Points);

                return new TopStudentDto
                {
                    UserId = sac.UserId,
                    FullName = sac.FullName,
                    AvatarUrl = sac.AvatarUrl,
                    ActivityCount = sac.ActivityCount,
                    TotalPointsAwarded = pointsAwarded
                };
            })
            .OrderByDescending(x => x.ActivityCount)
            .ThenByDescending(x => x.TotalPointsAwarded)
            .Take(10)
            .ToList();

        // So sánh với năm học trước
        var previousAcademicYear = await _context.AcademicYears
            .Where(ay => ay.EndDate < academicYear.StartDate)
            .OrderByDescending(ay => ay.EndDate)
            .FirstOrDefaultAsync();

        if (previousAcademicYear != null)
        {
            var prevClasses = await _context.ClassGroups
                .Where(cg => cg.AcademicYearId == previousAcademicYear.Id && !cg.IsDeleted)
                .CountAsync();

            var prevStudents = await _context.ClassGroups
                .Where(cg => cg.AcademicYearId == previousAcademicYear.Id && !cg.IsDeleted)
                .SelectMany(cg => cg.ClassGroupMembers)
                .Select(m => m.UserId)
                .Distinct()
                .CountAsync();

            var prevActivities = await _context.Activities
                .Where(a => !a.IsDeleted &&
                           ((a.StartDate.HasValue && a.StartDate >= previousAcademicYear.StartDate && a.StartDate <= previousAcademicYear.EndDate) ||
                            (a.EndDate.HasValue && a.EndDate >= previousAcademicYear.StartDate && a.EndDate <= previousAcademicYear.EndDate)))
                .CountAsync();

            var prevParticipants = await _context.ActivityParticipants
                .Where(ap => ap.Status == ParticipantStatus.Joined &&
                            ap.Activity != null &&
                            !ap.Activity.IsDeleted &&
                            ((ap.Activity.StartDate.HasValue && ap.Activity.StartDate >= previousAcademicYear.StartDate && ap.Activity.StartDate <= previousAcademicYear.EndDate) ||
                             (ap.Activity.EndDate.HasValue && ap.Activity.EndDate >= previousAcademicYear.StartDate && ap.Activity.EndDate <= previousAcademicYear.EndDate)))
                .Select(ap => ap.UserId)
                .Distinct()
                .CountAsync();

            result.ComparisonWithPreviousYear = new AcademicYearComparisonDto
            {
                PreviousAcademicYearId = previousAcademicYear.Id,
                PreviousAcademicYearName = previousAcademicYear.Name,
                ClassCountChangePercent = prevClasses > 0 ? ((double)(classes.Count - prevClasses) / prevClasses) * 100 : null,
                StudentCountChangePercent = prevStudents > 0 ? ((double)(totalStudents - prevStudents) / prevStudents) * 100 : null,
                ActivityCountChangePercent = prevActivities > 0 ? ((double)(activities.Count - prevActivities) / prevActivities) * 100 : null,
                ParticipantCountChangePercent = prevParticipants > 0 ? ((double)(students.Count - prevParticipants) / prevParticipants) * 100 : null
            };
        }

        return result;
    }

    public async Task<ClassGroupStatisticsDto> GetClassGroupStatisticsAsync(int classGroupId, int? academicYearId = null)
    {
        var classGroup = await _context.ClassGroups
            .Include(cg => cg.ClassGroupMembers)
                .ThenInclude(m => m.User)
            .Include(cg => cg.AcademicYears)
            .FirstOrDefaultAsync(cg => cg.Id == classGroupId && !cg.IsDeleted);

        if (classGroup == null)
        {
            throw new KeyNotFoundException($"ClassGroup with ID {classGroupId} not found");
        }

        var targetAcademicYearId = academicYearId ?? classGroup.AcademicYearId;
        var academicYear = targetAcademicYearId.HasValue
            ? await _context.AcademicYears.FindAsync(targetAcademicYearId.Value)
            : null;

        var studentIds = classGroup.ClassGroupMembers.Select(m => m.UserId).ToList();
        var totalStudents = studentIds.Count;

        // Lấy các activities mà lớp đã tham gia
        var activityParticipants = await _context.ActivityParticipants
            .Where(ap => ap.ClassGroupId == classGroupId &&
                        ap.Status == ParticipantStatus.Joined)
            .Include(ap => ap.Activity)
            .ToListAsync();

        var activities = activityParticipants
            .Select(ap => ap.Activity)
            .Where(a => a != null && !a.IsDeleted)
            .Distinct()
            .ToList();

        // Filter by academic year if provided
        if (academicYear != null)
        {
            activities = activities.Where(a =>
                (a.StartDate.HasValue && a.StartDate >= academicYear.StartDate && a.StartDate <= academicYear.EndDate) ||
                (a.EndDate.HasValue && a.EndDate >= academicYear.StartDate && a.EndDate <= academicYear.EndDate) ||
                (a.StartDate.HasValue && a.EndDate.HasValue && a.StartDate <= academicYear.EndDate && a.EndDate >= academicYear.StartDate)
            ).ToList();
        }

        var totalActivitiesParticipated = activities.Count;

        // Tính tổng điểm lớp đã nhận (chỉ tính điểm Earn và loại trừ điểm âm)
        var totalPointsAwarded = await _context.PointHistory
            .Where(ph => studentIds.Contains(ph.UserId) &&
                        ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0 && // Chỉ tính điểm dương
                        (academicYear == null || (ph.CreatedAt >= academicYear.StartDate && ph.CreatedAt <= academicYear.EndDate)))
            .SumAsync(ph => ph.Points);

        var averagePoints = totalStudents > 0 ? (double)totalPointsAwarded / totalStudents : 0;

        // Tỷ lệ tham gia
        var studentsParticipated = activityParticipants.Select(ap => ap.UserId).Distinct().Count();
        var participationRate = totalStudents > 0 ? (double)studentsParticipated / totalStudents * 100 : 0;

        // Số giải thưởng đạt được
        var rewards = await _context.ActivityRewards
            .Where(ar => !ar.IsDeleted &&
                        activities.Select(a => a.Id).Contains(ar.ActivityId))
            .ToListAsync();

        var totalRewardsWon = rewards.Count;

        var result = new ClassGroupStatisticsDto
        {
            ClassGroupId = classGroup.Id,
            ClassGroupName = classGroup.Name ?? "N/A",
            Grade = classGroup.Grade,
            AcademicYearId = targetAcademicYearId,
            AcademicYearName = academicYear?.Name,
            TotalStudents = totalStudents,
            TotalActivitiesParticipated = totalActivitiesParticipated,
            TotalPointsAwarded = totalPointsAwarded,
            AveragePoints = Math.Round(averagePoints, 2),
            ParticipationRate = Math.Round(participationRate, 2),
            TotalRewardsWon = totalRewardsWon
        };

        // Top sự kiện lớp đạt giải - Tính điểm từ PointHistory thay vì từ ActivityRewards để tránh đúp
        var rewardActivityIds = rewards.Select(r => r.ActivityId).Distinct().ToList();
        var rewardActivityTitles = activities
            .Where(a => rewardActivityIds.Contains(a.Id) && !string.IsNullOrEmpty(a.Title))
            .Select(a => a.Title)
            .Distinct()
            .ToList();
        
        // Lấy tất cả điểm từ PointHistory cho các activities có giải thưởng
        var allRewardPoints = await _context.PointHistory
            .Where(ph => studentIds.Contains(ph.UserId) &&
                        ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0 &&
                        ph.Description != null &&
                        (rewardActivityIds.Any(id => ph.Description.Contains($"Activity {id}")) ||
                         rewardActivityTitles.Any(title => ph.Description.Contains(title!))) &&
                        (academicYear == null || (ph.CreatedAt >= academicYear.StartDate && ph.CreatedAt <= academicYear.EndDate)))
            .ToListAsync();

        // Group điểm theo activity trong memory
        var rewardPointsByActivity = rewardActivityIds.ToDictionary(
            activityId => activityId,
            activityId =>
            {
                var activity = activities.FirstOrDefault(a => a.Id == activityId);
                var activityTitle = activity?.Title;
                
                return allRewardPoints
                    .Where(ph => ph.Description != null &&
                                (ph.Description.Contains($"Activity {activityId}") ||
                                 (!string.IsNullOrEmpty(activityTitle) && ph.Description.Contains(activityTitle))))
                    .Sum(ph => ph.Points);
            }
        );

        result.TopRewardActivities = rewards
            .Join(activities, r => r.ActivityId, a => a.Id, (r, a) => new ClassGroupRewardActivityDto
            {
                ActivityId = a.Id,
                ActivityTitle = a.Title ?? "N/A",
                Rank = r.Rank,
                PointsAwarded = rewardPointsByActivity.ContainsKey(a.Id) ? rewardPointsByActivity[a.Id] : 0, // Lấy từ PointHistory
                ActivityEndDate = a.EndDate
            })
            .Where(x => x.PointsAwarded > 0) // Chỉ lấy những activity có điểm thực tế
            .OrderByDescending(x => x.PointsAwarded)
            .Take(10)
            .ToList();

        // Xếp hạng lớp trong năm học
        if (academicYear != null)
        {
            var allClassesInYear = await _context.ClassGroups
                .Where(cg => cg.AcademicYearId == academicYear.Id && !cg.IsDeleted)
                .ToListAsync();

            // Load tất cả student IDs của các lớp trong năm học trước
            var allClassGroupIds = allClassesInYear.Select(cg => cg.Id).ToList();
            var allClassGroupMembers = await _context.ClassGroupMembers
                .Where(m => allClassGroupIds.Contains(m.ClassGroupId))
                .ToListAsync();

            // Load tất cả points trong năm học trước (chỉ điểm Earn và dương)
            var allStudentIdsInYear = allClassGroupMembers.Select(m => m.UserId).Distinct().ToList();
            var allPointsInYear = await _context.PointHistory
                .Where(ph => allStudentIdsInYear.Contains(ph.UserId) &&
                            ph.CreatedAt >= academicYear.StartDate &&
                            ph.CreatedAt <= academicYear.EndDate &&
                            ph.ActionType == PointActionType.Earn &&
                            ph.Points > 0) // Chỉ tính điểm dương
                .ToListAsync();

            // Tính điểm cho từng lớp trong memory (tránh concurrent DbContext access)
            var classRankings = allClassesInYear.Select(cg =>
            {
                var cgStudentIds = allClassGroupMembers
                    .Where(m => m.ClassGroupId == cg.Id)
                    .Select(m => m.UserId)
                    .ToList();

                var cgPoints = allPointsInYear
                    .Where(ph => cgStudentIds.Contains(ph.UserId))
                    .Sum(ph => ph.Points);

                return new { ClassGroupId = cg.Id, TotalPoints = cgPoints };
            }).ToList();

            var ranking = classRankings
                .OrderByDescending(x => x.TotalPoints)
                .Select((x, index) => new { x.ClassGroupId, Rank = index + 1 })
                .FirstOrDefault(x => x.ClassGroupId == classGroupId);

            result.RankingInAcademicYear = ranking?.Rank;
            result.TotalClassesInAcademicYear = allClassesInYear.Count;
        }

        // Phân bổ điểm theo học sinh
        // Load tất cả users và points trước để tránh concurrent DbContext access
        var allUsers = await _context.Users
            .Where(u => studentIds.Contains(u.Id))
            .ToListAsync();

        var allStudentPoints = await _context.PointHistory
            .Where(ph => studentIds.Contains(ph.UserId) &&
                        ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0 && // Chỉ tính điểm dương
                        (academicYear == null || (ph.CreatedAt >= academicYear.StartDate && ph.CreatedAt <= academicYear.EndDate)))
            .ToListAsync();

        // Tính điểm cho từng học sinh trong memory (tránh concurrent DbContext access)
        var studentPointsList = studentIds.Select(userId =>
        {
            var user = allUsers.FirstOrDefault(u => u.Id == userId);
            var points = allStudentPoints
                .Where(ph => ph.UserId == userId)
                .Sum(ph => ph.Points);

            var activityCount = activityParticipants.Count(ap => ap.UserId == userId);

            return new StudentPointsDto
            {
                UserId = userId,
                FullName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "N/A",
                AvatarUrl = user?.AvatarUrl,
                TotalPoints = points,
                ActivityCount = activityCount
            };
        }).ToList();

        result.StudentPointsDistribution = studentPointsList.OrderByDescending(s => s.TotalPoints).ToList();

        // Phân bổ tham gia theo loại sự kiện
        result.ParticipationByActivityType = activities
            .GroupBy(a => a.SubType ?? "Unknown")
            .Select(g => new ActivityTypeParticipationDto
            {
                ActivityType = g.Key,
                ActivityCount = g.Count(),
                ParticipantCount = activityParticipants.Count(ap => g.Select(a => a.Id).Contains(ap.ActivityId)),
                TotalPointsAwarded = 0 // Can be calculated separately if needed
            })
            .ToList();

        return result;
    }

    public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(int? academicYearId = null)
    {
        var now = DateTime.UtcNow;

        // User counts
        var userCounts = new UserCountDto
        {
            TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
            Students = await _context.Users.CountAsync(u => !u.IsDeleted && u.Role == UserRole.Student),
            Teachers = await _context.Users.CountAsync(u => !u.IsDeleted && u.Role == UserRole.Teacher),
            Admins = await _context.Users.CountAsync(u => !u.IsDeleted && u.Role == UserRole.Admin)
        };

        // System counts
        var systemCounts = new SystemCountDto
        {
            TotalClasses = await _context.ClassGroups.CountAsync(cg => !cg.IsDeleted),
            TotalAcademicYears = await _context.AcademicYears.CountAsync(),
            ActiveClubs = await _context.Clubs.CountAsync(c => !c.IsDeleted && c.ClubMembers.Any(cm => !cm.IsDeleted))
        };

        // Activity counts
        var activitiesQuery = _context.Activities.Where(a => !a.IsDeleted);
        if (academicYearId.HasValue)
        {
            var academicYear = await _context.AcademicYears.FindAsync(academicYearId.Value);
            if (academicYear != null)
            {
                activitiesQuery = activitiesQuery.Where(a =>
                    (a.StartDate.HasValue && a.StartDate >= academicYear.StartDate && a.StartDate <= academicYear.EndDate) ||
                    (a.EndDate.HasValue && a.EndDate >= academicYear.StartDate && a.EndDate <= academicYear.EndDate) ||
                    (a.StartDate.HasValue && a.EndDate.HasValue && a.StartDate <= academicYear.EndDate && a.EndDate >= academicYear.StartDate)
                );
            }
        }

        var activities = await activitiesQuery.ToListAsync();
        var activityCounts = new ActivityCountDto
        {
            TotalActivities = activities.Count,
            Ongoing = activities.Count(a => a.StartDate.HasValue && a.StartDate <= now && a.EndDate.HasValue && a.EndDate >= now),
            Upcoming = activities.Count(a => a.StartDate.HasValue && a.StartDate > now),
            Completed = activities.Count(a => a.EndDate.HasValue && a.EndDate < now)
        };

        // Total points awarded (chỉ tính điểm Earn và dương)
        var totalPointsAwarded = await _context.PointHistory
            .Where(ph => ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0 && // Chỉ tính điểm dương
                        (academicYearId == null || 
                         (academicYearId.HasValue && _context.AcademicYears.Any(ay => 
                            ay.Id == academicYearId.Value &&
                            ph.CreatedAt >= ay.StartDate &&
                            ph.CreatedAt <= ay.EndDate))))
            .SumAsync(ph => ph.Points);

        // Activity timeline (last 12 months)
        var timelineStart = now.AddMonths(-12);
        var timelineData = await _context.Activities
            .Where(a => !a.IsDeleted && a.StartDate.HasValue && a.StartDate >= timelineStart)
            .GroupBy(a => new { Year = a.StartDate!.Value.Year, Month = a.StartDate.Value.Month })
            .Select(g => new
            {
                Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                ActivityCount = g.Count(),
                ActivityIds = g.Select(a => a.Id).ToList()
            })
            .ToListAsync();

        // Load tất cả participants và points trước để tránh concurrent DbContext access
        var timelineActivityIds = timelineData.SelectMany(td => td.ActivityIds).Distinct().ToList();
        var timelineParticipants = await _context.ActivityParticipants
            .Where(ap => timelineActivityIds.Contains(ap.ActivityId) && ap.Status == ParticipantStatus.Joined)
            .ToListAsync();

        var timelinePoints = await _context.PointHistory
            .Where(ph => ph.CreatedAt >= timelineStart &&
                        ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0) // Chỉ tính điểm dương
            .ToListAsync();

        // Tính toán trong memory (tránh concurrent DbContext access)
        var activityTimeline = timelineData.Select(td =>
        {
            var participantCount = timelineParticipants
                .Count(ap => td.ActivityIds.Contains(ap.ActivityId));

            var pointsAwarded = timelinePoints
                .Where(ph => ph.CreatedAt.Year == td.Date.Year &&
                            ph.CreatedAt.Month == td.Date.Month)
                .Sum(ph => ph.Points);

            return new ActivityTimelineDto
            {
                Date = td.Date,
                MonthYear = $"{td.Date.Month}/{td.Date.Year}",
                ActivityCount = td.ActivityCount,
                ParticipantCount = participantCount,
                PointsAwarded = pointsAwarded
            };
        }).OrderBy(x => x.Date).ToList();

        // Top 10 lớp tích cực nhất
        var topClasses = await _context.ActivityParticipants
            .Where(ap => ap.ClassGroupId.HasValue &&
                        ap.Status == ParticipantStatus.Joined &&
                        ap.Activity != null &&
                        !ap.Activity.IsDeleted)
            .GroupBy(ap => new { ap.ClassGroupId, ap.ClassGroup!.Name })
            .Select(g => new
            {
                ClassGroupId = g.Key.ClassGroupId!.Value,
                ClassGroupName = g.Key.Name ?? "N/A",
                ActivityCount = g.Select(ap => ap.ActivityId).Distinct().Count(),
                ParticipantCount = g.Count(),
                ParticipantUserIds = g.Select(ap => ap.UserId).Distinct().ToList()
            })
            .OrderByDescending(x => x.ActivityCount)
            .Take(10)
            .ToListAsync();

        // Load tất cả points trước để tránh concurrent DbContext access (chỉ điểm Earn và dương)
        var topClassParticipantIds = topClasses.SelectMany(tc => tc.ParticipantUserIds).Distinct().ToList();
        var topClassPoints = await _context.PointHistory
            .Where(ph => topClassParticipantIds.Contains(ph.UserId) &&
                        ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0) // Chỉ tính điểm dương
            .ToListAsync();

        // Tính toán trong memory (tránh concurrent DbContext access)
        var topActiveClasses = topClasses.Select(tc =>
        {
            var pointsAwarded = topClassPoints
                .Where(ph => tc.ParticipantUserIds.Contains(ph.UserId))
                .Sum(ph => ph.Points);

            return new TopClassGroupDto
            {
                ClassGroupId = tc.ClassGroupId,
                ClassGroupName = tc.ClassGroupName,
                ActivityCount = tc.ActivityCount,
                ParticipantCount = tc.ParticipantCount,
                TotalPointsAwarded = pointsAwarded
            };
        }).ToList();

        // Top 10 học sinh tích cực nhất
        var topStudents = await _context.ActivityParticipants
            .Where(ap => ap.Status == ParticipantStatus.Joined &&
                        ap.Activity != null &&
                        !ap.Activity.IsDeleted)
            .GroupBy(ap => new { ap.UserId, ap.User!.FirstName, ap.User.LastName, ap.User.AvatarUrl })
            .Select(g => new
            {
                UserId = g.Key.UserId,
                FullName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                AvatarUrl = g.Key.AvatarUrl,
                ActivityCount = g.Select(ap => ap.ActivityId).Distinct().Count()
            })
            .OrderByDescending(x => x.ActivityCount)
            .Take(10)
            .ToListAsync();

        // Load tất cả points trước để tránh concurrent DbContext access (chỉ điểm Earn và dương)
        var topStudentIds = topStudents.Select(ts => ts.UserId).Distinct().ToList();
        var topStudentPoints = await _context.PointHistory
            .Where(ph => topStudentIds.Contains(ph.UserId) &&
                        ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0) // Chỉ tính điểm dương
            .ToListAsync();

        // Tính toán trong memory (tránh concurrent DbContext access)
        var topActiveStudents = topStudents.Select(ts =>
        {
            var pointsAwarded = topStudentPoints
                .Where(ph => ph.UserId == ts.UserId)
                .Sum(ph => ph.Points);

            return new TopStudentDto
            {
                UserId = ts.UserId,
                FullName = ts.FullName,
                AvatarUrl = ts.AvatarUrl,
                ActivityCount = ts.ActivityCount,
                TotalPointsAwarded = pointsAwarded
            };
        }).ToList();

        // Phân bổ điểm theo năm học
        var academicYears = await _context.AcademicYears.ToListAsync();
        
        // Load tất cả dữ liệu cần thiết trước để tránh concurrent DbContext access
        var allActivities = await _context.Activities
            .Where(a => !a.IsDeleted)
            .ToListAsync();

        var allActivityParticipants = await _context.ActivityParticipants
            .Where(ap => ap.Status == ParticipantStatus.Joined)
            .ToListAsync();

        var allPointHistory = await _context.PointHistory
            .Where(ph => ph.ActionType == PointActionType.Earn &&
                        ph.Points > 0) // Chỉ tính điểm dương
            .ToListAsync();

        // Tính toán trong memory (tránh concurrent DbContext access)
        var pointsByAcademicYear = academicYears.Select(ay =>
        {
            var activityIds = allActivities
                .Where(a => (a.StartDate.HasValue && a.StartDate >= ay.StartDate && a.StartDate <= ay.EndDate) ||
                           (a.EndDate.HasValue && a.EndDate >= ay.StartDate && a.EndDate <= ay.EndDate))
                .Select(a => a.Id)
                .ToList();

            var participantIds = allActivityParticipants
                .Where(ap => activityIds.Contains(ap.ActivityId))
                .Select(ap => ap.UserId)
                .Distinct()
                .ToList();

            var totalPoints = allPointHistory
                .Where(ph => participantIds.Contains(ph.UserId) &&
                            ph.CreatedAt >= ay.StartDate &&
                            ph.CreatedAt <= ay.EndDate)
                .Sum(ph => ph.Points);

            return new PointsByAcademicYearDto
            {
                AcademicYearId = ay.Id,
                AcademicYearName = ay.Name,
                TotalPoints = totalPoints,
                ActivityCount = activityIds.Count,
                ParticipantCount = participantIds.Count
            };
        }).ToList();

        return new DashboardStatisticsDto
        {
            UserCounts = userCounts,
            SystemCounts = systemCounts,
            ActivityCounts = activityCounts,
            TotalPointsAwarded = totalPointsAwarded,
            ActivityTimeline = activityTimeline,
            TopActiveClasses = topActiveClasses,
            TopActiveStudents = topActiveStudents,
            PointsByAcademicYear = pointsByAcademicYear
        };
    }
}

