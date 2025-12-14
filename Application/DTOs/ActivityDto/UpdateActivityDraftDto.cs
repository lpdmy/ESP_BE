using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class UpdateActivityDraftDto
    {
        [Required]
        public int Id { get; set; }

        [StringLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự")]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public ActivityType? Category { get; set; }

        [StringLength(100)]
        public string? SubType { get; set; }

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

        public string? GradingSettings { get; set; }

        public string? RegistrationSettings { get; set; }

        public bool? OnlyTeacherCanRegister { get; set; }

        public string? StarPointRewards { get; set; }

        // Collections
        public List<string>? Rules { get; set; }

        public List<string>? SportsCategories { get; set; }

        public List<ActivitySportConfigDto>? SportsConfigurations { get; set; }

        public List<ActivitySpeakerDto>? Speakers { get; set; }

        public List<ActivityProgramDto>? ProgramItems { get; set; }

        // Draft-specific fields
        [StringLength(255)]
        public string? DraftName { get; set; }

        public string? Notes { get; set; }
    }
}

