using System;
using System.Collections.Generic;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    /// <summary>
    /// DTO tối ưu cho danh sách activities - chỉ chứa các field cần thiết cho list view
    /// </summary>
    public class ActivityListItemDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? EndRegisterDate { get; set; }
        public string? Location { get; set; }
        public string Organizer { get; set; } = null!;
        public int? MaxParticipants { get; set; }
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public int NumberOfParticipants { get; set; }
        public string? Status { get; set; }
        public bool? HasSubmitted { get; set; }

        // Additional fields for admin list view
        public bool OnlyTeacherCanRegister { get; set; }
        public string? GradingSettings { get; set; }
        public ActivityRegistrationSettingsDto? RegistrationSettings { get; set; }
        public List<ActivitySportDto> Sports { get; set; } = new();
        public List<ActivityParticipantDto> Participants { get; set; } = new();
        public bool IsDeleted { get; set; }
        
        // Check if current user is registered for this activity
        public bool IsRegistered { get; set; } = false;
    }
}

