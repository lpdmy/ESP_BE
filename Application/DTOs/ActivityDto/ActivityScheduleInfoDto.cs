using System;
using System.Collections.Generic;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// DTO lightweight cho AISchedule - chỉ chứa các trường cần thiết để tối ưu performance
/// </summary>
public class ActivityScheduleInfoDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    
    /// <summary>
    /// Danh sách môn thể thao - chỉ chứa id và sportName
    /// </summary>
    public List<ActivitySportBasicDto> Sports { get; set; } = new();
    
    /// <summary>
    /// Danh sách participants - chỉ chứa classGroupId và isDeleted
    /// </summary>
    public List<ActivityParticipantBasicDto> Participants { get; set; } = new();
}

/// <summary>
/// DTO cơ bản cho Sport - chỉ chứa id và sportName
/// </summary>
public class ActivitySportBasicDto
{
    public int Id { get; set; }
    public string SportName { get; set; } = null!;
}

/// <summary>
/// DTO cơ bản cho Participant - chỉ chứa classGroupId và isDeleted
/// </summary>
public class ActivityParticipantBasicDto
{
    public int? ClassGroupId { get; set; }
    public bool IsDeleted { get; set; }
}

