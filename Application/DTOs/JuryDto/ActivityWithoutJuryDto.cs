using System;

namespace EduShpere.Application.DTOs
{
    /// <summary>
    /// DTO for activities that don't have any assigned jurors
    /// </summary>
    public class ActivityWithoutJuryDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public int SubmissionCount { get; set; } // Số lượng bài nộp
        public bool HasSubmissions { get; set; } // Có bài nộp hay chưa
        public DateTime CreatedAt { get; set; }
    }
}

