using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class UpdateActivityDto
    {
        [Required(ErrorMessage = "ID là bắt buộc")]
        public int Id { get; set; }
        
        [StringLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự")]
        public string? Title { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string? Location { get; set; }
        
        public int? ClubId { get; set; }
        
        public string? Organizer { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Số người tham gia tối đa phải lớn hơn 0")]
        public int? MaxParticipants { get; set; }
        
        public DateTime? RegisterDate { get; set; }
        
        public DateTime? EndRegisterDate { get; set; }
        
        public ActivityType? Category { get; set; }
        
        public string? SubType { get; set; }
        
        public string? ThumbnailUrl { get; set; }
        
        // Rules
        public List<string>? Rules { get; set; }
        
        // SportsFestival fields
        public List<string>? SportsCategories { get; set; }
        public List<ActivitySportConfigDto>? SportsConfigurations { get; set; }
        public string? CompetitionType { get; set; }
        
        // CreativeContest fields
        public string? Theme { get; set; }
        public string? Genre { get; set; }
        public string? PaperSize { get; set; }
        public string? DrawingMedium { get; set; }
        public string? TimeLimit { get; set; }
        public string? SubmissionFormat { get; set; }
        
        // SeminarWorkshop fields
        public List<ActivitySpeakerDto>? Speakers { get; set; }
        public List<ActivityProgramDto>? ProgramItems { get; set; }
        
        // StarPoint Rewards
        public ActivityRegistrationRewardDto? RegistrationReward { get; set; }
        public List<ActivityAwardDto>? Awards { get; set; }
        
        // Helper property for frontend compatibility (maps to Awards)
        public StarPointRewardsDto? StarPointRewards { get; set; }
        
        // Grading Settings (will be serialized to JSON string)
        public GradingSettingsDto? GradingSettings { get; set; }
        
        // Registration Settings
        public bool? OnlyTeacherCanRegister { get; set; }
        public ActivityRegistrationSettingsDto? RegistrationSettings { get; set; }
        
        // Problem/Submission fields - chỉ áp dụng cho Activity có nộp bài (CreativeContest hoặc SubType có submission)
        public string? ProblemText { get; set; } // Đề bài (text)
        public string? ProblemFileUrl { get; set; } // URL hoặc path đến file đề bài
        public DateTime? SubmissionDeadline { get; set; } // Hạn cuối nộp bài - phải >= StartDate và <= EndDate
        
        // Soft delete flag
        public bool? IsDeleted { get; set; }
    }
}
