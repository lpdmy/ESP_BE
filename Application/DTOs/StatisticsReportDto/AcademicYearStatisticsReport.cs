namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class AcademicYearStatisticsReport
{
    public string AcademicYear { get; set; } = string.Empty;

    public SchoolOverviewMetrics Overview { get; set; } = new();

    public IReadOnlyList<ClassYearScoreSummary> Classes { get; set; } = new List<ClassYearScoreSummary>();
    public IReadOnlyList<StudentYearScoreSummary> Students { get; set; } = new List<StudentYearScoreSummary>();

    public WeeklyTrendMetrics WeeklyTrends { get; set; } = new();
    public ScoreDistributionMetrics ScoreDistribution { get; set; } = new();
    public IReadOnlyList<ActivityTypeStatistics> ActivityTypeStatistics { get; set; } = new List<ActivityTypeStatistics>();
}

