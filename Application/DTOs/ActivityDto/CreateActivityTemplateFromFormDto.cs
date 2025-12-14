using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// DTO để tạo ActivityTemplate từ form data (thay thế cho CreateActivityDraftDto)
/// </summary>
public class CreateActivityTemplateFromFormDto
{
    [Required(ErrorMessage = ErrorMessages.Validation.FieldRequired)]
    [StringLength(500, ErrorMessage = "Tên mẫu không được vượt quá 500 ký tự")]
    public string TemplateName { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? TemplateDescription { get; set; }

    [Required(ErrorMessage = "Phân loại hoạt động là bắt buộc")]
    [StringLength(100)]
    public string SubType { get; set; } = null!;

    // Form data fields - sẽ được chuyển thành PrefillData dictionary
    [StringLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public ActivityType Category { get; set; } = ActivityType.Activity;

    [StringLength(500)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Organizer { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? RegisterDate { get; set; }

    public DateTime? EndRegisterDate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số người tham gia tối đa phải lớn hơn 0")]
    public int? MaxParticipants { get; set; }

    // SportsFestival fields
    [StringLength(50)]
    public string? CompetitionType { get; set; }

    // CreativeContest fields
    [StringLength(500)]
    public string? Theme { get; set; }

    [StringLength(200)]
    public string? Genre { get; set; }

    [StringLength(200)]
    public string? PaperSize { get; set; }

    [StringLength(200)]
    public string? DrawingMedium { get; set; }

    [StringLength(200)]
    public string? TimeLimit { get; set; }

    [StringLength(500)]
    public string? SubmissionFormat { get; set; }

    // Problem/Submission fields
    public string? ProblemText { get; set; }

    public string? ProblemFileUrl { get; set; }

    public DateTime? SubmissionDeadline { get; set; }

    // Settings
    public bool? IsGrade { get; set; }

    public string? GradingSettings { get; set; } // JSON string

    public string? RegistrationSettings { get; set; } // JSON string

    public bool? OnlyTeacherCanRegister { get; set; }

    public string? StarPointRewards { get; set; } // JSON string

    // Collections
    public List<string>? Rules { get; set; }

    public List<string>? SportsCategories { get; set; }

    public List<ActivitySportConfigDto>? SportsConfigurations { get; set; }

    public List<ActivitySpeakerDto>? Speakers { get; set; }

    public List<ActivityProgramDto>? ProgramItems { get; set; }

    // Template-specific: Checklist
    public List<string>? Checklist { get; set; }
}

