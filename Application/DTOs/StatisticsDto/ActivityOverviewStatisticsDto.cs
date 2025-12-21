namespace EduShpere.Application.DTOs.StatisticsDto;

/// <summary>
/// DTO cho thống kê tổng quan về sự kiện
/// </summary>
public class ActivityOverviewStatisticsDto
{
    /// <summary>
    /// Tổng số sự kiện đã tạo
    /// </summary>
    public int TotalCreated { get; set; }
    
    /// <summary>
    /// Số sự kiện đang diễn ra
    /// </summary>
    public int Ongoing { get; set; }
    
    /// <summary>
    /// Số sự kiện đã kết thúc
    /// </summary>
    public int Completed { get; set; }
    
    /// <summary>
    /// Số sự kiện đã hủy
    /// </summary>
    public int Cancelled { get; set; }
    
    /// <summary>
    /// Phân loại sự kiện theo loại (SubType)
    /// </summary>
    public List<ActivityTypeCountDto> ByType { get; set; } = new();
    
    /// <summary>
    /// Top sự kiện có nhiều người tham gia nhất
    /// </summary>
    public List<TopActivityDto> TopActivitiesByParticipants { get; set; } = new();
    
    /// <summary>
    /// Phân bổ số lượng sự kiện theo năm học
    /// </summary>
    public List<ActivityByAcademicYearDto> ByAcademicYear { get; set; } = new();
    
    /// <summary>
    /// Phân bổ số lượng sự kiện theo tháng (trong năm học hiện tại)
    /// </summary>
    public List<ActivityByMonthDto> ByMonth { get; set; } = new();
}

public class ActivityTypeCountDto
{
    public string Type { get; set; } = null!; // SubType
    public int Count { get; set; }
    public int TotalParticipants { get; set; }
}

public class TopActivityDto
{
    public int ActivityId { get; set; }
    public string Title { get; set; } = null!;
    public int ParticipantCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ActivityByAcademicYearDto
{
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = null!;
    public int ActivityCount { get; set; }
    public int TotalParticipants { get; set; }
}

public class ActivityByMonthDto
{
    public int Month { get; set; }
    public string MonthName { get; set; } = null!;
    public int ActivityCount { get; set; }
    public int TotalParticipants { get; set; }
}


