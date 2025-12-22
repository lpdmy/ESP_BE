namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class ClassScorePoint
{
    public string ClassCode { get; set; } = string.Empty;
    public int? Grade { get; set; }
    public decimal TotalScoreInYear { get; set; }
    public int ActivityCountInYear { get; set; }
}

