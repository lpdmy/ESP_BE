namespace EduShpere.Application.DTOs.StatisticsDto;

/// <summary>
/// DTO cho thống kê chi tiết của 1 sự kiện cụ thể
/// </summary>
public class ActivityDetailStatisticsDto
{
    public int ActivityId { get; set; }
    public string ActivityTitle { get; set; } = null!;
    
    /// <summary>
    /// Tổng số người đăng ký
    /// </summary>
    public int TotalRegistered { get; set; }
    
    /// <summary>
    /// Số người tham gia thực tế (Status = Joined)
    /// </summary>
    public int ActualParticipants { get; set; }
    
    /// <summary>
    /// Tỷ lệ tham gia (% so với đăng ký)
    /// </summary>
    public double ParticipationRate { get; set; }
    
    /// <summary>
    /// Tổng điểm thưởng đã trao cho người tham gia
    /// </summary>
    public int TotalPointsAwarded { get; set; }
    
    /// <summary>
    /// Điểm trung bình mỗi người tham gia
    /// </summary>
    public double AveragePointsPerParticipant { get; set; }
    
    /// <summary>
    /// Số giải thưởng đã trao
    /// </summary>
    public int TotalRewardsAwarded { get; set; }
    
    /// <summary>
    /// Phân bổ theo lớp (ClassGroup)
    /// </summary>
    public List<ClassGroupParticipationDto> ParticipationByClassGroup { get; set; } = new();
    
    /// <summary>
    /// Dữ liệu cho biểu đồ Registered vs Actual Participants theo thời gian
    /// </summary>
    public List<ParticipationTimelineDto> ParticipationTimeline { get; set; } = new();
}

public class ClassGroupParticipationDto
{
    public int ClassGroupId { get; set; }
    public string ClassGroupName { get; set; } = null!;
    public int RegisteredCount { get; set; }
    public int ActualParticipantsCount { get; set; }
    public int PointsAwarded { get; set; }
}

public class ParticipationTimelineDto
{
    public DateTime Date { get; set; }
    public int RegisteredCount { get; set; }
    public int ActualParticipantsCount { get; set; }
}


