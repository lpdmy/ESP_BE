using System;
using System.Collections.Generic;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class MyActivityResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public string Organizer { get; set; } = null!;
        public int? MaxParticipants { get; set; }
        public int NumberOfParticipants { get; set; }
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public DateTime RegisterDate { get; set; }
        public DateTime EndRegisterDate { get; set; }
        
        // Problem/Submission fields - chỉ áp dụng cho Activity có nộp bài
        public DateTime? SubmissionDeadline { get; set; } // Hạn cuối nộp bài
        public string? ProblemText { get; set; } // Đề bài (text) - null nếu chưa đến thời gian mở đề
        public string? ProblemFileUrl { get; set; } // URL hoặc path đến file đề bài - null nếu chưa đến thời gian mở đề
        
        // User participation info
        public DateTime? RegisteredAt { get; set; }
        public int? StarPoints { get; set; }
        public ParticipantStatus? ParticipationStatus { get; set; }
        
        // Related data (simplified for list view)
        public List<string> Rules { get; set; } = new();
    }
}

