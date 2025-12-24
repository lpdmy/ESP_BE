using System;
using System.Collections.Generic;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// DTO lightweight cho Activity View Detail - chỉ chứa các trường cần thiết để tối ưu performance
/// </summary>
public class ActivityViewInfoDto
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
    public string ThumbnailUrl { get; set; } = null!;
    public DateTime RegisterDate { get; set; }
    public DateTime EndRegisterDate { get; set; }
    public DateTime? SubmissionDeadline { get; set; }
    public string? ProblemText { get; set; }
    public string? ProblemFileUrl { get; set; }
    public bool IsProblemVisible { get; set; }
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Danh sách môn thể thao - chỉ chứa các trường cần thiết
    /// </summary>
    public List<ActivitySportViewDto> Sports { get; set; } = new();
    
    /// <summary>
    /// Danh sách participants - chỉ chứa các trường cần thiết
    /// </summary>
    public List<ActivityParticipantViewDto> Participants { get; set; } = new();
    
    /// <summary>
    /// Danh sách rules - chỉ chứa text
    /// </summary>
    public List<string> Rules { get; set; } = new();
    
    /// <summary>
    /// Danh sách speakers - chỉ chứa các trường cần thiết (cho SeminarWorkshop)
    /// </summary>
    public List<ActivitySpeakerViewDto> Speakers { get; set; } = new();
    
    /// <summary>
    /// Danh sách programs - chỉ chứa các trường cần thiết (cho SeminarWorkshop)
    /// </summary>
    public List<ActivityProgramViewDto> Programs { get; set; } = new();
    
    /// <summary>
    /// Activity detail - chỉ chứa các trường cần thiết
    /// </summary>
    public ActivityDetailViewDto? ActivityDetail { get; set; }
}

/// <summary>
/// DTO cho Sport trong view - chỉ chứa các trường cần thiết
/// </summary>
public class ActivitySportViewDto
{
    public int Id { get; set; }
    public string SportName { get; set; } = null!;
    public int? MaxMembers { get; set; }
}

/// <summary>
/// DTO cho Participant trong view - chỉ chứa các trường cần thiết
/// </summary>
public class ActivityParticipantViewDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ClassGroupId { get; set; }
    public int? SportId { get; set; }
    public string? Status { get; set; }
    public bool IsDeleted { get; set; }
    public string? UserFullName { get; set; }
    public string? UserAvatarUrl { get; set; }
    public string? ClassGroupName { get; set; }
    public int? Grade { get; set; }
    public string? SportName { get; set; }
}

/// <summary>
/// DTO cho Speaker trong view - chỉ chứa các trường cần thiết
/// </summary>
public class ActivitySpeakerViewDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? ImageUrl { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// DTO cho Program trong view - chỉ chứa các trường cần thiết
/// </summary>
public class ActivityProgramViewDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Time { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// DTO cho ActivityDetail trong view - chỉ chứa các trường cần thiết
/// </summary>
public class ActivityDetailViewDto
{
    public int Id { get; set; }
    public string? CompetitionType { get; set; }
    public string? Theme { get; set; }
    public string? Genre { get; set; }
    public string? PaperSize { get; set; }
    public string? DrawingMedium { get; set; }
    public string? TimeLimit { get; set; }
    public string? SubmissionFormat { get; set; }
}

