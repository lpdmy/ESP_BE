using System;
using System.Collections.Generic;

namespace EduShpere.Application.DTOs.SubmissionDto
{
    /// <summary>
    /// DTO for anonymous submission display (for jurors)
    /// Only shows submission code/order number, hides user information
    /// </summary>
    public class SubmissionAnonymousDto
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string SubmissionCode { get; set; } // Mã bài nộp (VD: SUB-001, SUB-002)
        public int OrderNumber { get; set; } // Số thứ tự
        public string Title { get; set; }
        public int NumberJurys { get; set; }
        public string? FileUrl { get; set; }
        public string? GradeSettings { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Score { get; set; }
        public string Status { get; set; }
        public List<JuryAssignmentDto> JuryAssignments { get; set; }
        public List<SubmissionAttachmentDto> Attachments { get; set; } = new List<SubmissionAttachmentDto>();
        
        // Hidden fields for anonymous mode:
        // - UserId
        // - FirstName, LastName
        // - Class
        // - UserFullName
        // - Users (jury user IDs)
    }
}

