using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("CreatedBy", Name = "IX_ActivityDrafts_CreatedBy")]
[Index("CreatedAt", Name = "IX_ActivityDrafts_CreatedAt")]
[Index("IsDeleted", Name = "IX_ActivityDrafts_IsDeleted")]
public class ActivityDraft : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public ActivityType Category { get; set; } = ActivityType.Activity;

    [Required]
    [StringLength(100)]
    public string SubType { get; set; } = null!; // SportsFestival, CreativeContest, SeminarWorkshop, Other

    [StringLength(500)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Organizer { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? RegisterDate { get; set; }

    public DateTime? EndRegisterDate { get; set; }

    public int? MaxParticipants { get; set; }

    // SportsFestival fields
    [StringLength(50)]
    public string? CompetitionType { get; set; } // Individual, Team, Mixed

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
    public bool? IsGrade { get; set; } = false;

    public string? GradingSettings { get; set; } // JSON string

    public string? RegistrationSettings { get; set; } // JSON string

    public bool? OnlyTeacherCanRegister { get; set; } = false;

    public string? StarPointRewards { get; set; } // JSON string

    // Collections stored as JSON
    public string? Rules { get; set; } // JSON array of strings

    public string? SportsCategories { get; set; } // JSON array of strings

    public string? SportsConfigurations { get; set; } // JSON array of objects

    public string? Speakers { get; set; } // JSON array of objects

    public string? ProgramItems { get; set; } // JSON array of objects

    // Draft-specific fields
    [StringLength(255)]
    public string? DraftName { get; set; } // Tên bản nháp (do user đặt)

    public string? Notes { get; set; } // Ghi chú của user

    // Navigation properties
    [ForeignKey("CreatedBy")]
    [InverseProperty("ActivityDrafts")]
    public virtual User? CreatedByUser { get; set; }
}

