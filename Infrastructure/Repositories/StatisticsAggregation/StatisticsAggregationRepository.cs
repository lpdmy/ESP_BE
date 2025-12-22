using EduShpere.Domain;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories.StatisticsAggregation;

public class StatisticsAggregationRepository : IStatisticsAggregationRepository
{
    private readonly EduShpereDbContext _context;

    public StatisticsAggregationRepository(EduShpereDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolOverviewMetricsDto> GetSchoolOverviewMetricsAsync(
        string academicYear, 
        DateTime startDate, 
        DateTime endDate)
    {
        var totalClasses = await _context.ClassGroups
            .Where(c => !c.IsDeleted && c.AcademicYearId.HasValue)
            .Join(_context.AcademicYears.Where(ay => ay.Name == academicYear),
                c => c.AcademicYearId,
                ay => ay.Id,
                (c, ay) => c)
            .CountAsync();

        var totalStudents = await _context.ClassGroupMembers
            .Where(m => !m.IsDeleted)
            .Join(_context.Users.Where(u => u.Role == UserRole.Student),
                m => m.UserId,
                u => u.Id,
                (m, u) => m)
            .Join(_context.ClassGroups.Where(c => !c.IsDeleted && c.AcademicYearId.HasValue),
                m => m.ClassGroupId,
                c => c.Id,
                (m, c) => c)
            .Join(_context.AcademicYears.Where(ay => ay.Name == academicYear),
                c => c.AcademicYearId,
                ay => ay.Id,
                (c, ay) => c)
            .SelectMany(c => c.ClassGroupMembers.Where(m => !m.IsDeleted))
            .Select(m => m.UserId)
            .Distinct()
            .CountAsync();

        var pointHistoryInYear = await _context.PointHistory
            .Include(ph => ph.User)
            .Where(ph => ph.CreatedAt >= startDate 
                && ph.CreatedAt <= endDate
                && ph.ActionType == PointActionType.Earn
                && ph.Points > 0
                && ph.User != null
                && ph.User.Role == UserRole.Student)
            .ToListAsync();

        var totalSchoolScore = pointHistoryInYear.Sum(ph => ph.Points);

        var classScores = await GetClassScoresAsync(academicYear, startDate, endDate);
        var classesWithZeroScore = classScores.Count(c => c.TotalScore == 0);

        var studentScores = await GetStudentScoresAsync(academicYear, startDate, endDate);
        var studentsWithZeroScore = studentScores.Count(s => s.TotalScore == 0);

        var averageScorePerClass = totalClasses > 0 ? totalSchoolScore / totalClasses : 0;
        var averageScorePerStudent = totalStudents > 0 ? totalSchoolScore / totalStudents : 0;

        var totalActivitiesInYear = await _context.Activities
            .Where(a => !a.IsDeleted
                && a.StartDate.HasValue
                && a.StartDate >= startDate
                && a.StartDate <= endDate)
            .CountAsync();

        const decimal topClassPercentageThreshold = 20m;
        var topClassesScoreShare = CalculateTopClassesScoreShare(classScores, totalSchoolScore, topClassPercentageThreshold);

        return new SchoolOverviewMetricsDto
        {
            TotalClasses = totalClasses,
            TotalStudents = totalStudents,
            ClassesWithZeroScore = classesWithZeroScore,
            StudentsWithZeroScore = studentsWithZeroScore,
            TotalSchoolScore = totalSchoolScore,
            AverageScorePerClass = averageScorePerClass,
            AverageScorePerStudent = averageScorePerStudent,
            TopClassPercentageThreshold = topClassPercentageThreshold,
            TopClassesScoreSharePercentage = topClassesScoreShare,
            TotalActivitiesInYear = totalActivitiesInYear
        };
    }

    public async Task<List<ClassYearScoreSummaryDto>> GetClassYearScoreSummariesAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate,
        bool includeAllClasses)
    {
        var classScores = await GetClassScoresAsync(academicYear, startDate, endDate);
        
        var classIds = classScores.Select(c => c.ClassId).ToList();
        
        var classes = await _context.ClassGroups
            .Where(c => classIds.Contains(c.Id) || includeAllClasses)
            .Include(c => c.Teacher)
            .Include(c => c.ClassGroupMembers.Where(m => !m.IsDeleted))
            .ToListAsync();

        var totalSchoolScore = classScores.Sum(c => c.TotalScore);
        
        var activityCountsByClass = await _context.ActivityParticipants
            .Where(ap => !ap.IsDeleted
                && ap.ClassGroupId.HasValue
                && ap.Activity != null
                && !ap.Activity.IsDeleted
                && ap.Activity.StartDate.HasValue
                && ap.Activity.StartDate >= startDate
                && ap.Activity.StartDate <= endDate)
            .GroupBy(ap => ap.ClassGroupId.Value)
            .Select(g => new { ClassId = g.Key, Count = g.Select(ap => ap.ActivityId).Distinct().Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count);

        var result = new List<ClassYearScoreSummaryDto>();
        var sortedClasses = classScores.OrderByDescending(c => c.TotalScore).ToList();

        for (int i = 0; i < sortedClasses.Count; i++)
        {
            var classScore = sortedClasses[i];
            var classGroup = classes.FirstOrDefault(c => c.Id == classScore.ClassId);
            
            if (classGroup == null) continue;

            var studentCount = classGroup.ClassGroupMembers.Count(m => !m.IsDeleted);
            var averageScorePerStudent = studentCount > 0 ? classScore.TotalScore / studentCount : 0;
            var rank = i + 1;
            var percentile = sortedClasses.Count > 0 ? (decimal)(sortedClasses.Count - rank) / sortedClasses.Count * 100 : 0;
            var scoreShare = totalSchoolScore > 0 ? (classScore.TotalScore / totalSchoolScore) * 100 : 0;

            result.Add(new ClassYearScoreSummaryDto
            {
                ClassId = classGroup.Id,
                ClassCode = classGroup.Name ?? $"Lớp {classGroup.Id}",
                Grade = classGroup.Grade,
                HomeroomTeacherName = classGroup.Teacher != null 
                    ? $"{classGroup.Teacher.FirstName} {classGroup.Teacher.LastName}".Trim() 
                    : "Chưa phân công",
                StudentCount = studentCount,
                TotalScoreInYear = classScore.TotalScore,
                AverageScorePerStudent = averageScorePerStudent,
                HasAnyScore = classScore.TotalScore > 0,
                ActivityCountInYear = activityCountsByClass.GetValueOrDefault(classGroup.Id, 0),
                RankByScore = rank,
                PercentileByScore = percentile,
                ScoreShareOfSchoolTotal = scoreShare
            });
        }

        if (includeAllClasses)
        {
            var classesWithoutScore = classes
                .Where(c => !classIds.Contains(c.Id))
                .Select(c => new ClassYearScoreSummaryDto
                {
                    ClassId = c.Id,
                    ClassCode = c.Name ?? $"Lớp {c.Id}",
                    Grade = c.Grade,
                    HomeroomTeacherName = c.Teacher != null 
                        ? $"{c.Teacher.FirstName} {c.Teacher.LastName}".Trim() 
                        : "Chưa phân công",
                    StudentCount = c.ClassGroupMembers.Count(m => !m.IsDeleted),
                    TotalScoreInYear = 0,
                    AverageScorePerStudent = 0,
                    HasAnyScore = false,
                    ActivityCountInYear = 0,
                    RankByScore = sortedClasses.Count + 1,
                    PercentileByScore = 0,
                    ScoreShareOfSchoolTotal = 0
                });
            
            result.AddRange(classesWithoutScore);
        }

        return result.OrderByDescending(c => c.TotalScoreInYear).ToList();
    }

    public async Task<List<StudentYearScoreSummaryDto>> GetStudentYearScoreSummariesAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate,
        bool includeAllStudents)
    {
        var studentScores = await GetStudentScoresAsync(academicYear, startDate, endDate);
        
        var studentIds = studentScores.Select(s => s.StudentId).ToList();
        
        var students = await _context.Users
            .Where(u => (studentIds.Contains(u.Id) || includeAllStudents)
                && u.Role == UserRole.Student)
            .Include(u => u.StudentProfile)
            .ToListAsync();

        var classMembers = await _context.ClassGroupMembers
            .Where(m => !m.IsDeleted)
            .Include(m => m.ClassGroup)
            .ToListAsync();

        var activityCountsByStudent = await _context.ActivityParticipants
            .Where(ap => !ap.IsDeleted
                && ap.Activity != null
                && !ap.Activity.IsDeleted
                && ap.Activity.StartDate.HasValue
                && ap.Activity.StartDate >= startDate
                && ap.Activity.StartDate <= endDate)
            .GroupBy(ap => ap.UserId)
            .Select(g => new { StudentId = g.Key, Count = g.Select(ap => ap.ActivityId).Distinct().Count() })
            .ToDictionaryAsync(x => x.StudentId, x => x.Count);

        var result = new List<StudentYearScoreSummaryDto>();

        foreach (var studentScore in studentScores)
        {
            var student = students.FirstOrDefault(u => u.Id == studentScore.StudentId);
            if (student == null) continue;

            var classMember = classMembers.FirstOrDefault(m => m.UserId == student.Id);
            var classGroup = classMember?.ClassGroup;

            result.Add(new StudentYearScoreSummaryDto
            {
                StudentId = student.Id,
                StudentCode = student.StudentProfile?.StudentNumber ?? student.Id.ToString(),
                StudentFullName = student != null 
                    ? $"{student.FirstName} {student.LastName}".Trim() 
                    : "Chưa có tên",
                ClassId = classGroup?.Id ?? 0,
                ClassCode = classGroup?.Name ?? "Chưa có lớp",
                Grade = classGroup?.Grade,
                TotalScoreInYear = studentScore.TotalScore,
                HasAnyScore = studentScore.TotalScore > 0,
                ActivityCountInYear = activityCountsByStudent.GetValueOrDefault(student.Id, 0)
            });
        }

        if (includeAllStudents)
        {
            var studentsWithoutScore = students
                .Where(u => !studentIds.Contains(u.Id))
                .Select(u =>
                {
                    var classMember = classMembers.FirstOrDefault(m => m.UserId == u.Id);
                    var classGroup = classMember?.ClassGroup;
                    
                    return new StudentYearScoreSummaryDto
                    {
                        StudentId = u.Id,
                        StudentCode = u.StudentProfile?.StudentNumber ?? u.Id.ToString(),
                        StudentFullName = u != null 
                            ? $"{u.FirstName} {u.LastName}".Trim() 
                            : "Chưa có tên",
                        ClassId = classGroup?.Id ?? 0,
                        ClassCode = classGroup?.Name ?? "Chưa có lớp",
                        Grade = classGroup?.Grade,
                        TotalScoreInYear = 0,
                        HasAnyScore = false,
                        ActivityCountInYear = 0
                    };
                });
            
            result.AddRange(studentsWithoutScore);
        }

        return result.OrderByDescending(s => s.TotalScoreInYear).ToList();
    }

    public async Task<List<WeeklyTrendPointDto>> GetWeeklyTrendPointsAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate)
    {
        var weeks = CalculateWeeks(startDate, endDate);
        var result = new List<WeeklyTrendPointDto>();

        foreach (var week in weeks)
        {
            var activitiesInWeek = await _context.Activities
                .Where(a => !a.IsDeleted
                    && a.StartDate.HasValue
                    && a.StartDate >= week.StartDate
                    && a.StartDate <= week.EndDate)
                .CountAsync();

            var pointsInWeek = await _context.PointHistory
                .Include(ph => ph.User)
                .Where(ph => ph.CreatedAt >= week.StartDate
                    && ph.CreatedAt <= week.EndDate
                    && ph.ActionType == PointActionType.Earn
                    && ph.Points > 0
                    && ph.User != null
                    && ph.User.Role == UserRole.Student)
                .SumAsync(ph => (decimal?)ph.Points) ?? 0;

            var participatedStudents = await _context.ActivityParticipants
                .Where(ap => !ap.IsDeleted
                    && ap.Activity != null
                    && !ap.Activity.IsDeleted
                    && ap.Activity.StartDate.HasValue
                    && ap.Activity.StartDate >= week.StartDate
                    && ap.Activity.StartDate <= week.EndDate)
                .Select(ap => ap.UserId)
                .Distinct()
                .CountAsync();

            result.Add(new WeeklyTrendPointDto
            {
                WeekIndex = week.WeekIndex,
                WeekStartDate = week.StartDate,
                WeekEndDate = week.EndDate,
                ActivityCount = activitiesInWeek,
                TotalScoreInWeek = pointsInWeek,
                ParticipatedStudentCount = participatedStudents
            });
        }

        return result;
    }

    public async Task<ScoreDistributionMetricsDto> GetScoreDistributionMetricsAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate,
        decimal topClassPercentageThreshold)
    {
        var classScores = await GetClassScoresAsync(academicYear, startDate, endDate);
        var totalSchoolScore = classScores.Sum(c => c.TotalScore);

        var classes = await _context.ClassGroups
            .Where(c => classScores.Select(cs => cs.ClassId).Contains(c.Id))
            .ToListAsync();

        var classScorePoints = classScores
            .OrderByDescending(c => c.TotalScore)
            .Select(cs =>
            {
                var classGroup = classes.FirstOrDefault(c => c.Id == cs.ClassId);
                return new ClassScorePointDto
                {
                    ClassCode = classGroup?.Name ?? $"Lớp {cs.ClassId}",
                    Grade = classGroup?.Grade,
                    TotalScoreInYear = cs.TotalScore,
                    ActivityCountInYear = 0
                };
            })
            .ToList();

        var activityCountsByClass = await _context.ActivityParticipants
            .Where(ap => !ap.IsDeleted
                && ap.ClassGroupId.HasValue
                && ap.Activity != null
                && !ap.Activity.IsDeleted
                && ap.Activity.StartDate.HasValue
                && ap.Activity.StartDate >= startDate
                && ap.Activity.StartDate <= endDate)
            .GroupBy(ap => ap.ClassGroupId.Value)
            .Select(g => new { ClassId = g.Key, Count = g.Select(ap => ap.ActivityId).Distinct().Count() })
            .ToDictionaryAsync(x => x.ClassId, x => x.Count);

        foreach (var point in classScorePoints)
        {
            var classGroup = classes.FirstOrDefault(c => c.Name == point.ClassCode);
            if (classGroup != null)
            {
                point.ActivityCountInYear = activityCountsByClass.GetValueOrDefault(classGroup.Id, 0);
            }
        }

        var topClassesScoreShare = CalculateTopClassesScoreShare(classScores, totalSchoolScore, topClassPercentageThreshold);
        var remainingClassesScoreShare = 100 - topClassesScoreShare;

        return new ScoreDistributionMetricsDto
        {
            AcademicYear = academicYear,
            ClassScorePoints = classScorePoints,
            TopClassPercentageThreshold = topClassPercentageThreshold,
            TopClassesScoreSharePercentage = topClassesScoreShare,
            RemainingClassesScoreSharePercentage = remainingClassesScoreShare
        };
    }

    public async Task<List<ActivityTypeStatisticsDto>> GetActivityTypeStatisticsAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate)
    {
        // Get activities in the date range (activities don't have AcademicYearId, filter by date only)
        var activities = await _context.Activities
            .Where(a => !a.IsDeleted
                && a.StartDate.HasValue
                && a.StartDate >= startDate
                && a.StartDate <= endDate)
            .ToListAsync();

        // Define 4 main activity types with Vietnamese names
        var activityTypeMapping = new Dictionary<string, string>
        {
            { "CreativeContest", "Cuộc thi sáng tạo" },
            { "SportsFestival", "Hội thao" },
            { "SeminarWorkshop", "Hội thảo - Workshop" },
            { "Other", "Hoạt động khác" }
        };

        // Filter to only 4 main types and group by SubType (ignore Category)
        var activityGroups = activities
            .Where(a => activityTypeMapping.ContainsKey(a.SubType ?? ""))
            .GroupBy(a => a.SubType ?? "")
            .Select(g => new
            {
                SubType = g.Key,
                ActivityIds = g.Select(a => a.Id).ToList(),
                Count = g.Count()
            })
            .ToList();

        // Get all point history in the date range (for approximation)
        var allPointHistory = await _context.PointHistory
            .Where(ph => ph.CreatedAt >= startDate
                && ph.CreatedAt <= endDate
                && ph.User.Role == UserRole.Student
                && ph.Points > 0
                && ph.ActionType == PointActionType.Earn)
            .ToListAsync();

        var totalPointsInRange = allPointHistory.Sum(ph => (decimal)ph.Points);
        var totalActivities = activities.Count;

        var result = new List<ActivityTypeStatisticsDto>();

        // Calculate total score proportionally based on activity count
        // This is an approximation since PointHistory doesn't have ActivityId
        foreach (var group in activityGroups)
        {
            // Approximate: distribute total points proportionally by activity count
            var approximateScore = totalActivities > 0 
                ? (totalPointsInRange * group.Count) / totalActivities 
                : 0;

            var vietnameseName = activityTypeMapping.GetValueOrDefault(group.SubType, group.SubType);
            
            result.Add(new ActivityTypeStatisticsDto
            {
                ActivityType = "", // Not used anymore, we only use SubType
                SubType = vietnameseName, // Store Vietnamese name in SubType
                Count = group.Count,
                TotalScore = approximateScore
            });
        }

        return result;
    }

    private async Task<List<(int ClassId, decimal TotalScore)>> GetClassScoresAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate)
    {
        var classScores = await _context.PointHistory
            .Include(ph => ph.User)
            .Where(ph => ph.CreatedAt >= startDate
                && ph.CreatedAt <= endDate
                && ph.ActionType == PointActionType.Earn
                && ph.Points > 0
                && ph.User != null
                && ph.User.Role == UserRole.Student)
            .Join(_context.ClassGroupMembers.Where(m => !m.IsDeleted),
                ph => ph.UserId,
                m => m.UserId,
                (ph, m) => new { ph, ClassId = m.ClassGroupId })
            .GroupBy(x => x.ClassId)
            .Select(g => new { ClassId = g.Key, TotalScore = g.Sum(x => (decimal)x.ph.Points) })
            .ToListAsync();

        return classScores.Select(cs => (cs.ClassId, cs.TotalScore)).ToList();
    }

    private async Task<List<(int StudentId, decimal TotalScore)>> GetStudentScoresAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate)
    {
        var studentScores = await _context.PointHistory
            .Include(ph => ph.User)
            .Where(ph => ph.CreatedAt >= startDate
                && ph.CreatedAt <= endDate
                && ph.ActionType == PointActionType.Earn
                && ph.Points > 0
                && ph.User != null
                && ph.User.Role == UserRole.Student)
            .GroupBy(ph => ph.UserId)
            .Select(g => new { StudentId = g.Key, TotalScore = g.Sum(ph => (decimal)ph.Points) })
            .ToListAsync();

        return studentScores.Select(ss => (ss.StudentId, ss.TotalScore)).ToList();
    }

    private decimal CalculateTopClassesScoreShare(
        List<(int ClassId, decimal TotalScore)> classScores,
        decimal totalSchoolScore,
        decimal topPercentageThreshold)
    {
        if (classScores.Count == 0 || totalSchoolScore == 0)
            return 0;

        var sortedClasses = classScores.OrderByDescending(c => c.TotalScore).ToList();
        var topCount = (int)Math.Ceiling(sortedClasses.Count * (double)topPercentageThreshold / 100);
        var topScoreSum = sortedClasses.Take(topCount).Sum(c => c.TotalScore);

        return (topScoreSum / totalSchoolScore) * 100;
    }

    private List<(int WeekIndex, DateTime StartDate, DateTime EndDate)> CalculateWeeks(
        DateTime startDate,
        DateTime endDate)
    {
        var weeks = new List<(int WeekIndex, DateTime StartDate, DateTime EndDate)>();
        var currentDate = startDate;
        int weekIndex = 1;

        while (currentDate <= endDate)
        {
            var weekStart = currentDate;
            var weekEnd = currentDate.AddDays(6);
            if (weekEnd > endDate)
                weekEnd = endDate;

            weeks.Add((weekIndex, weekStart, weekEnd));
            currentDate = weekEnd.AddDays(1);
            weekIndex++;
        }

        return weeks;
    }
}

