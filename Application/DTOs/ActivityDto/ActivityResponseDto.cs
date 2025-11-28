using System;
using System.Collections.Generic;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class ActivityResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public int? ClubId { get; set; }
        public string Organizer { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public int MaxParticipants { get; set; }
        public byte[] RowVersion { get; set; } = null!;
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public int NumberOfParticipants { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime EndRegisterDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Related data
        public List<string> Rules { get; set; } = new();
        public List<ActivityParticipantDto> Participants { get; set; } = new();
        
        // SportsFestival
        public List<ActivitySportDto> Sports { get; set; } = new();
        public ActivityDetailDto? ActivityDetail { get; set; }
        
        // SeminarWorkshop
        public List<ActivitySpeakerDto> Speakers { get; set; } = new();
        public List<ActivityProgramDto> Programs { get; set; } = new();
        
        // StarPoint Rewards
        public ActivityRegistrationRewardDto? RegistrationReward { get; set; }
        public List<ActivityAwardDto> Awards { get; set; } = new();
        
        // Grading Settings (Enabled is derived from IsGrade in Activity entity)
        public GradingSettingsDto? GradingSettings { get; set; }
        
        // Registration Settings
        public bool OnlyTeacherCanRegister { get; set; }
        public ActivityRegistrationSettingsDto? RegistrationSettings { get; set; }
    }
}
