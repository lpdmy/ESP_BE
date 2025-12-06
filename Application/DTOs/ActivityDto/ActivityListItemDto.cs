using System;
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
        public string? Location { get; set; }
        public string Organizer { get; set; } = null!;
        public int? MaxParticipants { get; set; }
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public int NumberOfParticipants { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime EndRegisterDate { get; set; }
        public string? Status { get; set; }
        
        // Registration Settings - chỉ cần cho CreativeContest
        public ActivityRegistrationSettingsDto? RegistrationSettings { get; set; }
        
        // Check if current user is registered for this activity
        public bool IsRegistered { get; set; } = false;
    }
}

