namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class WeeklyTrendPoint
{
    public int WeekIndex { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }

    public int ActivityCount { get; set; }
    public decimal TotalScoreInWeek { get; set; }
    public int ParticipatedStudentCount { get; set; }
}

