using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class JuryAssignment : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int SubmissionId { get; set; }
        public Submission Submission { get; set; }
        public string? ScoreTemp { get; set; }
        public string? Comment { get; set; }
        public int? TotalScore { get; set; }
    }
}
