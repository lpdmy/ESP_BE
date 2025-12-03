using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.ModerationDto
{
    public class AddModerationDto
    {
        public string? ContentText { get; set; }
        public int? AuthorId { get; set; }
        public int? ReporterId { get; set; }
        public string? Status { get; set; }
    }
}
