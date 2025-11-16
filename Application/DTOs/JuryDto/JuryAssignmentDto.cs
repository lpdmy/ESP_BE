using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.SubmissionDto;

namespace EduShpere.Application.DTOs
{
    public class JuryAssignmentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SubmissionId { get; set; }
        public SubmissionResponseDto Submission { get; set; }
        public float? ScoreTemp { get; set; }
    }
}
