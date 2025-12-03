using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class CreateActivityDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(500, ErrorMessage = "Tiêu đề không được vượt quá 500 ký tự")]
        public string Title { get; set; } = null!;
        
        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; } = null!;
        
        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        public DateTime EndDate { get; set; }
        
        [Required(ErrorMessage = "Địa điểm là bắt buộc")]
        [StringLength(20, ErrorMessage = "Địa điểm không được vượt quá 20 ký tự")]
        public string Location { get; set; } = null!;
        
        [Required(ErrorMessage = "Loại hoạt động là bắt buộc")]
        public ActivityType Category { get; set; }
        
        [Required(ErrorMessage = "Phân loại hoạt động là bắt buộc")]
        public string SubType { get; set; } = null!;
        
        [Required(ErrorMessage = "Ảnh đại diện là bắt buộc")]
        public string ThumbnailUrl { get; set; } = null!;
        
        [Required(ErrorMessage = "Đơn vị tổ chức là bắt buộc")]
        public string Organizer { get; set; } = null!;
        
        [Required(ErrorMessage = "Ngày mở đăng ký là bắt buộc")]
        public DateTime RegisterDate { get; set; }
        
        [Required(ErrorMessage = "Ngày đóng đăng ký là bắt buộc")]
        public DateTime EndRegisterDate { get; set; }
        
        [Required(ErrorMessage = "Số người tham gia tối đa là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người tham gia tối đa phải lớn hơn 0")]
        public int MaxParticipants { get; set; }
        
        public int? ClubId { get; set; }
        
        // Rules
        public List<string> Rules { get; set; } = new List<string>();
        
        // SportsFestival fields
        public List<string> SportsCategories { get; set; } = new List<string>();
        public List<ActivitySportConfigDto> SportsConfigurations { get; set; } = new List<ActivitySportConfigDto>();
        public string? CompetitionType { get; set; } // Individual, Team, Mixed
        
        // CreativeContest fields
        public string? Theme { get; set; }
        public string? Genre { get; set; }
        public string? PaperSize { get; set; }
        public string? DrawingMedium { get; set; }
        public string? TimeLimit { get; set; }
        public string? SubmissionFormat { get; set; }
        
        // SeminarWorkshop fields
        public List<ActivitySpeakerDto> Speakers { get; set; } = new List<ActivitySpeakerDto>();
        public List<ActivityProgramDto> ProgramItems { get; set; } = new List<ActivityProgramDto>();
        
        // StarPoint Rewards
        public ActivityRegistrationRewardDto? RegistrationReward { get; set; }
        public List<ActivityAwardDto> Awards { get; set; } = new List<ActivityAwardDto>();
        
        // Helper property for frontend compatibility (maps to Awards)
        public StarPointRewardsDto? StarPointRewards { get; set; }
        
        // Grading Settings (will be serialized to JSON string)
        public GradingSettingsDto? GradingSettings { get; set; }
        
        // Registration Settings
        public bool OnlyTeacherCanRegister { get; set; } = false;
        public ActivityRegistrationSettingsDto? RegistrationSettings { get; set; }
        
        // Problem/Submission fields - chỉ áp dụng cho Activity có nộp bài (CreativeContest hoặc SubType có submission)
        public string? ProblemText { get; set; } // Đề bài (text)
        public string? ProblemFileUrl { get; set; } // URL hoặc path đến file đề bài
        public DateTime? SubmissionDeadline { get; set; } // Hạn cuối nộp bài - phải >= StartDate và <= EndDate
    }
    
    public class GradingSettingsDto
    {
        // Note: Enabled is stored separately in Activity.IsGrade
        // This DTO only contains criteria
        public List<string> Criteria { get; set; } = new List<string>();
    }
    
    public class StarPointRewardsDto
    {
        public string? Registration { get; set; } // String for frontend compatibility
        public List<AwardDto> Awards { get; set; } = new List<AwardDto>();
    }
    
    public class AwardDto
    {
        public string? Name { get; set; }
        public string? Points { get; set; } // String for frontend compatibility
    }
}
