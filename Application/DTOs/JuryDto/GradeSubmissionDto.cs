using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class GradeSubmissionDto
    {
        public int id { get; set; }
        public Dictionary<string, int> Scores { get; set; } = new();
        public string? Comment { get; set; }
        public int TotalScore { get; set; }
    }
}
