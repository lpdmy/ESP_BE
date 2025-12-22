namespace EduShpere.Application.DTOs.StatisticsReportDto;

public class StatisticsReportRequest
{
    public string AcademicYear { get; set; } = string.Empty;
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IncludeClassRewardTable { get; set; } = true;
    public bool IncludeStudentRewardTable { get; set; } = true;
    public bool IncludeWeeklyTrendCharts { get; set; } = true;
    public bool IncludeDistributionCharts { get; set; } = true;

    public bool IncludeAllClasses { get; set; } = true;
    public bool IncludeAllStudents { get; set; } = true;
    
    // Chart images as base64 strings (PNG format)
    public string? WeeklyTrendChartImage { get; set; } // Base64 PNG image
    public string? ClassDistributionChartImage { get; set; } // Base64 PNG image
    public string? ScoreConcentrationChartImage { get; set; } // Base64 PNG image
    public string? ParticipationChartImage { get; set; } // Base64 PNG image
}

