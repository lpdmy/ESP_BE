namespace EduShpere.Application.DTOs.StatisticsDto;

/// <summary>
/// DTO cho thống kê theo năm học
/// </summary>
public class AcademicYearStatisticsDto
{
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Tổng số lớp trong năm học
    /// </summary>
    public int TotalClasses { get; set; }
    
    /// <summary>
    /// Tổng số học sinh trong năm học
    /// </summary>
    public int TotalStudents { get; set; }
    
    /// <summary>
    /// Tổng số sự kiện trong năm học
    /// </summary>
    public int TotalActivities { get; set; }
    
    /// <summary>
    /// Số câu lạc bộ đang hoạt động
    /// </summary>
    public int ActiveClubs { get; set; }
    
    /// <summary>
    /// Thống kê hoạt động theo từng tháng trong năm học
    /// </summary>
    public List<MonthlyActivityDto> MonthlyActivities { get; set; } = new();
    
    /// <summary>
    /// Top lớp tích cực nhất (theo số sự kiện tham gia)
    /// </summary>
    public List<TopClassGroupDto> TopActiveClasses { get; set; } = new();
    
    /// <summary>
    /// Top học sinh tích cực nhất (theo số sự kiện tham gia)
    /// </summary>
    public List<TopStudentDto> TopActiveStudents { get; set; } = new();
    
    /// <summary>
    /// So sánh với năm học trước (% tăng/giảm)
    /// </summary>
    public AcademicYearComparisonDto? ComparisonWithPreviousYear { get; set; }
}

public class MonthlyActivityDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public int ActivityCount { get; set; }
    public int ParticipantCount { get; set; }
    public int TotalPointsAwarded { get; set; }
}

public class TopClassGroupDto
{
    public int ClassGroupId { get; set; }
    public string ClassGroupName { get; set; } = null!;
    public int ActivityCount { get; set; }
    public int ParticipantCount { get; set; }
    public int TotalPointsAwarded { get; set; }
}

public class TopStudentDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public int ActivityCount { get; set; }
    public int TotalPointsAwarded { get; set; }
}

public class AcademicYearComparisonDto
{
    public int? PreviousAcademicYearId { get; set; }
    public string? PreviousAcademicYearName { get; set; }
    
    /// <summary>
    /// % thay đổi số lớp
    /// </summary>
    public double? ClassCountChangePercent { get; set; }
    
    /// <summary>
    /// % thay đổi số học sinh
    /// </summary>
    public double? StudentCountChangePercent { get; set; }
    
    /// <summary>
    /// % thay đổi số sự kiện
    /// </summary>
    public double? ActivityCountChangePercent { get; set; }
    
    /// <summary>
    /// % thay đổi số người tham gia
    /// </summary>
    public double? ParticipantCountChangePercent { get; set; }
}


