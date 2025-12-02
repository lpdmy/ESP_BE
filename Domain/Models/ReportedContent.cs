using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class ReportedContent
    {
        public int Id { get; set; }
        public string? ContentText { get; set; }
        public int? AuthorId { get; set; }
        public int? ReporterId { get; set; }
        public int? ReviewerId { get; set; }
        public string ? Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.Now;
        public User Author { get; set; }
        public User Reporter { get; set; }
        public User? Reviewer { get; set; }

    }
}
