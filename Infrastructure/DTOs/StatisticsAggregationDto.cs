namespace EduShpere.Infrastructure.DTOs;

public class SchoolOverviewMetricsDto
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

public class ClassYearScoreSummaryDto
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

public class StudentYearScoreSummaryDto
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

public class WeeklyTrendPointDto
{
    public int WeekIndex { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int ActivityCount { get; set; }
    public decimal TotalScoreInWeek { get; set; }
    public int ParticipatedStudentCount { get; set; }
}

public class ClassScorePointDto
{
    public string ClassCode { get; set; } = string.Empty;
    public int? Grade { get; set; }
    public decimal TotalScoreInYear { get; set; }
    public int ActivityCountInYear { get; set; }
}

public class ScoreDistributionMetricsDto
{
    public string AcademicYear { get; set; } = string.Empty;
    public List<ClassScorePointDto> ClassScorePoints { get; set; } = new();
    public decimal TopClassPercentageThreshold { get; set; }
    public decimal TopClassesScoreSharePercentage { get; set; }
    public decimal RemainingClassesScoreSharePercentage { get; set; }
}

public class ActivityTypeStatisticsDto
{
    public string ActivityType { get; set; } = string.Empty;
    public string SubType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalScore { get; set; }
}

