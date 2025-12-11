using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs;
using ClassGroupDtoAlias = EduShpere.Application.DTOs.ClassGroupDto.ClassGroupDto;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EduShpere.Application.DTOs.SubmissionDto
{
    public class SubmissionResponseDto
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string UserFullName => $"{LastName} {FirstName}".Trim();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int NumberJurys { get; set; }
        public ClassGroupDtoAlias Class { get; set; }
        
        /// <summary>
        /// Tên đầy đủ với lớp (nếu có). Format: "Tên - Lớp" hoặc chỉ "Tên" nếu không có lớp
        /// </summary>
        public string UserFullNameWithClass
        {
            get
            {
                var fullName = $"{LastName} {FirstName}".Trim();
                if (Class != null && !string.IsNullOrWhiteSpace(Class.Name))
                {
                    var className = Class.Grade.HasValue 
                        ? $"{Class.Grade.Value}{Class.Name}" 
                        : Class.Name;
                    return $"{fullName} - {className}";
                }
                return fullName;
            }
        }
        public List<int> Users { get; set; }
        public string? FileUrl { get; set; }
        public string? GradeSettings { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Score { get; set; }
        public string ActivityName { get; set; }
        public string Status { get; set; }
        public List<JuryAssignmentDto> JuryAssignments { get; set; }
        
        // Anonymous submission fields (for jurors)
        public string? SubmissionCode { get; set; } // Mã bài nộp (VD: SUB-001)
        public int? OrderNumber { get; set; } // Số thứ tự

        public List<SubmissionAttachmentDto> Attachments { get; set; } = new List<SubmissionAttachmentDto>();
    }
}
