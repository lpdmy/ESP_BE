namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class SchoolOverviewMetrics
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }

    public int ClassesWithZeroScore { get; set; }
    public int StudentsWithZeroScore { get; set; }

    public decimal TotalSchoolScore { get; set; }

    public decimal AverageScorePerClass { get; set; }
    public decimal AverageScorePerStudent { get; set; }

    public decimal TopClassPercentageThreshold { get; set; }
    public decimal TopClassesScoreSharePercentage { get; set; }

    public int TotalActivitiesInYear { get; set; }
}

