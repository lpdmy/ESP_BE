using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Application.DTOs
{
    public class ModerationResponseDto
    {
        public int Id { get; set; }
        public string? ContentText { get; set; }
        public int? AuthorId { get; set; }
        public int? ReporterId { get; set; }
        public int? ReviewerId { get; set; }
        public string? Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? AuthorName { get; set; }
        public string? ReporterName { get; set; }
    }
}
