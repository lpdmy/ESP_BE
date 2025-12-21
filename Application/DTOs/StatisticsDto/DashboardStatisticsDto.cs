namespace EduShpere.Application.DTOs.StatisticsDto;

/// <summary>
/// DTO cho Dashboard Admin tổng hợp
/// </summary>
public class DashboardStatisticsDto
{
    /// <summary>
    /// Tổng số người dùng: học sinh, giáo viên
    /// </summary>
    public UserCountDto UserCounts { get; set; } = new();
    
    /// <summary>
    /// Tổng số lớp, tổng số năm học
    /// </summary>
    public SystemCountDto SystemCounts { get; set; } = new();
    
    /// <summary>
    /// Tổng số sự kiện: đang diễn ra, sắp tới
    /// </summary>
    public ActivityCountDto ActivityCounts { get; set; } = new();
    
    /// <summary>
    /// Tổng điểm đã trao trong toàn hệ thống
    /// </summary>
    public int TotalPointsAwarded { get; set; }
    
    /// <summary>
    /// Biểu đồ hoạt động theo thời gian (theo tháng)
    /// </summary>
    public List<ActivityTimelineDto> ActivityTimeline { get; set; } = new();
    
    /// <summary>
    /// Top 10 lớp tích cực nhất
    /// </summary>
    public List<TopClassGroupDto> TopActiveClasses { get; set; } = new();
    
    /// <summary>
    /// Top 10 học sinh tích cực nhất
    /// </summary>
    public List<TopStudentDto> TopActiveStudents { get; set; } = new();
    
    /// <summary>
    /// Phân bổ điểm theo năm học
    /// </summary>
    public List<PointsByAcademicYearDto> PointsByAcademicYear { get; set; } = new();
}

public class UserCountDto
{
    public int TotalUsers { get; set; }
    public int Students { get; set; }
    public int Teachers { get; set; }
    public int Admins { get; set; }
}

public class SystemCountDto
{
    public int TotalClasses { get; set; }
    public int TotalAcademicYears { get; set; }
    public int ActiveClubs { get; set; }
}

public class ActivityCountDto
{
    public int TotalActivities { get; set; }
    public int Ongoing { get; set; }
    public int Upcoming { get; set; }
    public int Completed { get; set; }
}

public class ActivityTimelineDto
{
    public DateTime Date { get; set; }
    public string MonthYear { get; set; } = null!;
    public int ActivityCount { get; set; }
    public int ParticipantCount { get; set; }
    public int PointsAwarded { get; set; }
}

public class PointsByAcademicYearDto
{
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = null!;
    public int TotalPoints { get; set; }
    public int ActivityCount { get; set; }
    public int ParticipantCount { get; set; }
}


