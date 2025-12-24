using System;
using System.Collections.Generic;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// DTO lightweight cho Activity Register Form - chỉ chứa các trường cần thiết để tối ưu performance
/// </summary>
public class ActivityRegisterInfoDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public int? MaxParticipants { get; set; }
    public int NumberOfParticipants { get; set; }
    public ActivityType Category { get; set; }
    public string SubType { get; set; } = null!;
    public DateTime RegisterDate { get; set; }
    public DateTime EndRegisterDate { get; set; }
    public string? RegistrationSettings { get; set; } // JSON string
    
    /// <summary>
    /// Danh sách môn thể thao - chỉ chứa các trường cần thiết
    /// </summary>
    public List<ActivitySportRegisterDto> Sports { get; set; } = new();
    
    /// <summary>
    /// Danh sách participants - chỉ chứa userId và isDeleted để check đã đăng ký chưa
    /// </summary>
    public List<ActivityParticipantRegisterDto> Participants { get; set; } = new();
}

/// <summary>
/// DTO cho Sport trong register form - chỉ chứa các trường cần thiết
/// </summary>
public class ActivitySportRegisterDto
{
    public int Id { get; set; }
    public string SportName { get; set; } = null!;
    public int? MaxMembers { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO cho Participant trong register form - chỉ chứa userId và isDeleted
/// </summary>
public class ActivityParticipantRegisterDto
{
    public int UserId { get; set; }
    public bool IsDeleted { get; set; }
}

