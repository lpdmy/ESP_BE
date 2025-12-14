using System;
using System.Collections.Generic;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class ActivityDraftResponseDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public ActivityType Category { get; set; }

        public string SubType { get; set; } = null!;

        public string? Location { get; set; }

        public string? Organizer { get; set; }

        public string? ThumbnailUrl { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? RegisterDate { get; set; }

        public DateTime? EndRegisterDate { get; set; }

        public int? MaxParticipants { get; set; }

        // SportsFestival fields
        public string? CompetitionType { get; set; }

        // CreativeContest fields
        public string? Theme { get; set; }

        public string? Genre { get; set; }

        public string? PaperSize { get; set; }

        public string? DrawingMedium { get; set; }

        public string? TimeLimit { get; set; }

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
        public string? DraftName { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

