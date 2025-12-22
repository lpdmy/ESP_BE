using EduShpere.Infrastructure.DTOs;

namespace EduShpere.Infrastructure.Repositories.StatisticsAggregation;

public interface IStatisticsAggregationRepository
{
    Task<SchoolOverviewMetricsDto> GetSchoolOverviewMetricsAsync(string academicYear, DateTime startDate, DateTime endDate);
    
    Task<List<ClassYearScoreSummaryDto>> GetClassYearScoreSummariesAsync(
        string academicYear, 
        DateTime startDate, 
        DateTime endDate,
        bool includeAllClasses);
    
    Task<List<StudentYearScoreSummaryDto>> GetStudentYearScoreSummariesAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate,
        bool includeAllStudents);
    
    Task<List<WeeklyTrendPointDto>> GetWeeklyTrendPointsAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate);
    
    Task<ScoreDistributionMetricsDto> GetScoreDistributionMetricsAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate,
        decimal topClassPercentageThreshold);
    
    Task<List<ActivityTypeStatisticsDto>> GetActivityTypeStatisticsAsync(
        string academicYear,
        DateTime startDate,
        DateTime endDate);
}

