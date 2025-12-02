using System;

namespace EduShpere.Application.DTOs.SubmissionDto
{
    public class SubmissionStatusDto
    {
        public bool CanSubmit { get; set; }
        public DateTime? SubmissionDeadline { get; set; }
        public bool HasSubmission { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string? SubmissionFileUrl { get; set; }
        public int? SubmissionId { get; set; }
        public string? Message { get; set; } // Lý do không thể nộp (nếu CanSubmit = false)
    }
}


