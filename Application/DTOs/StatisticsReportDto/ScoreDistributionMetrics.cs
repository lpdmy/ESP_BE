namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class ScoreDistributionMetrics
{
    public string AcademicYear { get; set; } = string.Empty;

    public IReadOnlyList<ClassScorePoint> ClassScorePoints { get; set; } = new List<ClassScorePoint>();

    public decimal TopClassPercentageThreshold { get; set; }
    public decimal TopClassesScoreSharePercentage { get; set; }
    public decimal RemainingClassesScoreSharePercentage { get; set; }
}

