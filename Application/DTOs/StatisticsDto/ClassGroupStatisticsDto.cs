namespace EduShpere.Application.DTOs.StatisticsDto;

/// <summary>
/// DTO cho thống kê theo lớp
/// </summary>
public class ClassGroupStatisticsDto
{
    public int ClassGroupId { get; set; }
    public string ClassGroupName { get; set; } = null!;
    public int? Grade { get; set; }
    public int? AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }
    
    /// <summary>
    /// Số học sinh trong lớp
    /// </summary>
    public int TotalStudents { get; set; }
    
    /// <summary>
    /// Số sự kiện lớp đã tham gia
    /// </summary>
    public int TotalActivitiesParticipated { get; set; }
    
    /// <summary>
    /// Tổng điểm lớp đã nhận
    /// </summary>
    public int TotalPointsAwarded { get; set; }
    
    /// <summary>
    /// Điểm trung bình của lớp
    /// </summary>
    public double AveragePoints { get; set; }
    
    /// <summary>
    /// Tỷ lệ tham gia (% học sinh tham gia)
    /// </summary>
    public double ParticipationRate { get; set; }
    
    /// <summary>
    /// Số giải thưởng đạt được
    /// </summary>
    public int TotalRewardsWon { get; set; }
    
    /// <summary>
    /// Top sự kiện lớp đạt giải
    /// </summary>
    public List<ClassGroupRewardActivityDto> TopRewardActivities { get; set; } = new();
    
    /// <summary>
    /// Xếp hạng lớp trong năm học (theo điểm)
    /// </summary>
    public int? RankingInAcademicYear { get; set; }
    
    /// <summary>
    /// Tổng số lớp trong cùng năm học
    /// </summary>
    public int TotalClassesInAcademicYear { get; set; }
    
    /// <summary>
    /// Phân bổ điểm theo học sinh
    /// </summary>
    public List<StudentPointsDto> StudentPointsDistribution { get; set; } = new();
    
    /// <summary>
    /// Phân bổ tham gia theo loại sự kiện
    /// </summary>
    public List<ActivityTypeParticipationDto> ParticipationByActivityType { get; set; } = new();
}

public class ClassGroupRewardActivityDto
{
    public int ActivityId { get; set; }
    public string ActivityTitle { get; set; } = null!;
    public string? Rank { get; set; }
    public int PointsAwarded { get; set; }
    public DateTime? ActivityEndDate { get; set; }
}

public class StudentPointsDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public int TotalPoints { get; set; }
    public int ActivityCount { get; set; }
}

public class ActivityTypeParticipationDto
{
    public string ActivityType { get; set; } = null!;
    public int ActivityCount { get; set; }
    public int ParticipantCount { get; set; }
    public int TotalPointsAwarded { get; set; }
}


