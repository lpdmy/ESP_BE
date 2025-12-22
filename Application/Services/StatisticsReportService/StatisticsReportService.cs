using EduShpere.Application.DTOs.StatisticsReportDto;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.DTOs;
using EduShpere.Infrastructure.Repositories.StatisticsAggregation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace EduShpere.Application.Services.StatisticsReportService;

public class StatisticsReportService : IStatisticsReportService
{
    private readonly IStatisticsAggregationRepository _aggregationRepository;
    private readonly EduShpereDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public StatisticsReportService(
        IStatisticsAggregationRepository aggregationRepository,
        EduShpereDbContext context,
        IMemoryCache cache)
    {
        _aggregationRepository = aggregationRepository;
        _context = context;
        _cache = cache;
    }

    public async Task<AcademicYearStatisticsReport> GetAcademicYearReportAsync(StatisticsReportRequest request)
    {
        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(ay => ay.Name == request.AcademicYear);

        if (academicYear == null)
        {
            throw new ArgumentException($"Không tìm thấy năm học: {request.AcademicYear}");
        }

        // Use custom date range if provided, otherwise use academic year range
        DateTime startDate;
        DateTime endDate;
        
        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            startDate = request.StartDate.Value;
            endDate = request.EndDate.Value;
            
            // Validate date range
            if (startDate < academicYear.StartDate || endDate > academicYear.EndDate)
            {
                throw new ArgumentException($"Khoảng thời gian phải nằm trong năm học ({academicYear.StartDate:dd/MM/yyyy} - {academicYear.EndDate:dd/MM/yyyy})");
            }
            
            if (startDate > endDate)
            {
                throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc");
            }
        }
        else
        {
            startDate = academicYear.StartDate;
            endDate = academicYear.EndDate;
        }

        // Generate cache key based on request parameters
        var cacheKey = GenerateCacheKey(request, startDate, endDate);
        
        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out AcademicYearStatisticsReport? cachedReport) && cachedReport != null)
        {
            return cachedReport;
        }

        var overviewDto = await _aggregationRepository.GetSchoolOverviewMetricsAsync(
            request.AcademicYear,
            startDate,
            endDate);

        var overview = new SchoolOverviewMetrics
        {
            TotalClasses = overviewDto.TotalClasses,
            TotalStudents = overviewDto.TotalStudents,
            ClassesWithZeroScore = overviewDto.ClassesWithZeroScore,
            StudentsWithZeroScore = overviewDto.StudentsWithZeroScore,
            TotalSchoolScore = overviewDto.TotalSchoolScore,
            AverageScorePerClass = overviewDto.AverageScorePerClass,
            AverageScorePerStudent = overviewDto.AverageScorePerStudent,
            TopClassPercentageThreshold = overviewDto.TopClassPercentageThreshold,
            TopClassesScoreSharePercentage = overviewDto.TopClassesScoreSharePercentage,
            TotalActivitiesInYear = overviewDto.TotalActivitiesInYear
        };

        // Helper method to format class code with grade
        static string FormatClassCodeWithGrade(string classCode, int? grade)
        {
            if (string.IsNullOrEmpty(classCode))
                return classCode;
            
            if (grade.HasValue)
            {
                // Check if classCode already starts with grade (e.g., "10A1")
                if (classCode.StartsWith(grade.Value.ToString()))
                    return classCode;
                
                // Otherwise prepend grade (e.g., "A1" -> "10A1")
                return $"{grade.Value}{classCode}";
            }
            
            return classCode;
        }

        var classesDto = request.IncludeClassRewardTable
            ? await _aggregationRepository.GetClassYearScoreSummariesAsync(
                request.AcademicYear,
                startDate,
                endDate,
                request.IncludeAllClasses)
            : new List<ClassYearScoreSummaryDto>();

        var classes = classesDto.Select(c => new ClassYearScoreSummary
        {
            ClassId = c.ClassId,
            ClassCode = FormatClassCodeWithGrade(c.ClassCode, c.Grade),
            Grade = c.Grade,
            HomeroomTeacherName = c.HomeroomTeacherName,
            StudentCount = c.StudentCount,
            TotalScoreInYear = c.TotalScoreInYear,
            AverageScorePerStudent = c.AverageScorePerStudent,
            HasAnyScore = c.HasAnyScore,
            ActivityCountInYear = c.ActivityCountInYear,
            RankByScore = c.RankByScore,
            PercentileByScore = c.PercentileByScore,
            ScoreShareOfSchoolTotal = c.ScoreShareOfSchoolTotal
        }).ToList();

        var studentsDto = request.IncludeStudentRewardTable
            ? await _aggregationRepository.GetStudentYearScoreSummariesAsync(
                request.AcademicYear,
                startDate,
                endDate,
                request.IncludeAllStudents)
            : new List<StudentYearScoreSummaryDto>();

        var students = studentsDto.Select(s => new StudentYearScoreSummary
        {
            StudentId = s.StudentId,
            StudentCode = s.StudentCode,
            StudentFullName = s.StudentFullName,
            ClassId = s.ClassId,
            ClassCode = FormatClassCodeWithGrade(s.ClassCode, s.Grade),
            Grade = s.Grade,
            TotalScoreInYear = s.TotalScoreInYear,
            HasAnyScore = s.HasAnyScore,
            ActivityCountInYear = s.ActivityCountInYear
        }).ToList();

        var weeklyTrendsDto = request.IncludeWeeklyTrendCharts
            ? await _aggregationRepository.GetWeeklyTrendPointsAsync(
                request.AcademicYear,
                startDate,
                endDate)
            : new List<WeeklyTrendPointDto>();

        var weeklyTrends = new WeeklyTrendMetrics
        {
            AcademicYear = request.AcademicYear,
            Weeks = weeklyTrendsDto.Select(w => new WeeklyTrendPoint
            {
                WeekIndex = w.WeekIndex,
                WeekStartDate = w.WeekStartDate,
                WeekEndDate = w.WeekEndDate,
                ActivityCount = w.ActivityCount,
                TotalScoreInWeek = w.TotalScoreInWeek,
                ParticipatedStudentCount = w.ParticipatedStudentCount
            }).ToList()
        };

        var scoreDistributionDto = request.IncludeDistributionCharts
            ? await _aggregationRepository.GetScoreDistributionMetricsAsync(
                request.AcademicYear,
                startDate,
                endDate,
                overview.TopClassPercentageThreshold)
            : null;

        var scoreDistribution = scoreDistributionDto != null
            ? new ScoreDistributionMetrics
            {
                AcademicYear = scoreDistributionDto.AcademicYear,
                ClassScorePoints = scoreDistributionDto.ClassScorePoints.Select(cp => new ClassScorePoint
                {
                    ClassCode = FormatClassCodeWithGrade(cp.ClassCode, cp.Grade),
                    Grade = cp.Grade,
                    TotalScoreInYear = cp.TotalScoreInYear,
                    ActivityCountInYear = cp.ActivityCountInYear
                }).ToList(),
                TopClassPercentageThreshold = scoreDistributionDto.TopClassPercentageThreshold,
                TopClassesScoreSharePercentage = scoreDistributionDto.TopClassesScoreSharePercentage,
                RemainingClassesScoreSharePercentage = scoreDistributionDto.RemainingClassesScoreSharePercentage
            }
            : new ScoreDistributionMetrics { AcademicYear = request.AcademicYear };

        var activityTypeStatisticsDto = await _aggregationRepository.GetActivityTypeStatisticsAsync(
            request.AcademicYear,
            startDate,
            endDate);

        var activityTypeStatistics = activityTypeStatisticsDto
            .OrderByDescending(a => a.Count) // Sort by count descending
            .Select(a => new ActivityTypeStatistics
            {
                ActivityType = a.ActivityType,
                SubType = a.SubType, // Already contains Vietnamese name
                Count = a.Count,
                TotalScore = a.TotalScore
            })
            .ToList();

        var report = new AcademicYearStatisticsReport
        {
            AcademicYear = request.AcademicYear,
            Overview = overview,
            Classes = classes,
            Students = students,
            WeeklyTrends = weeklyTrends,
            ScoreDistribution = scoreDistribution,
            ActivityTypeStatistics = activityTypeStatistics
        };

        // Cache the result
        _cache.Set(cacheKey, report, CacheExpiration);

        return report;
    }

    private static string GenerateCacheKey(StatisticsReportRequest request, DateTime startDate, DateTime endDate)
    {
        var sb = new StringBuilder();
        sb.Append($"StatsReport_{request.AcademicYear}_");
        sb.Append($"{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_");
        sb.Append($"C{request.IncludeClassRewardTable}_");
        sb.Append($"S{request.IncludeStudentRewardTable}_");
        sb.Append($"W{request.IncludeWeeklyTrendCharts}_");
        sb.Append($"D{request.IncludeDistributionCharts}_");
        sb.Append($"AC{request.IncludeAllClasses}_");
        sb.Append($"AS{request.IncludeAllStudents}");
        return sb.ToString();
    }
}

