namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class ActivityTypeStatistics
{
    public string ActivityType { get; set; } = string.Empty;
    public string SubType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalScore { get; set; }
}

