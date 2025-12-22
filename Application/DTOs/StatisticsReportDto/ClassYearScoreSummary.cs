namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class ClassYearScoreSummary
{
    public int ClassId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public int? Grade { get; set; }
    public string HomeroomTeacherName { get; set; } = string.Empty;

    public int StudentCount { get; set; }

    public decimal TotalScoreInYear { get; set; }
    public decimal AverageScorePerStudent { get; set; }

    public bool HasAnyScore { get; set; }
    public int ActivityCountInYear { get; set; }

    public int RankByScore { get; set; }
    public decimal PercentileByScore { get; set; }

    public decimal ScoreShareOfSchoolTotal { get; set; }
}

