namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class WeeklyTrendMetrics
{
    public string AcademicYear { get; set; } = string.Empty;
    public IReadOnlyList<WeeklyTrendPoint> Weeks { get; set; } = new List<WeeklyTrendPoint>();
}

