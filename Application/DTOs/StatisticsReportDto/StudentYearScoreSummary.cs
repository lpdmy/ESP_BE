namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class StudentYearScoreSummary
{
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;

    public int ClassId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public int? Grade { get; set; }

    public decimal TotalScoreInYear { get; set; }
    public bool HasAnyScore { get; set; }

    public int ActivityCountInYear { get; set; }
}

