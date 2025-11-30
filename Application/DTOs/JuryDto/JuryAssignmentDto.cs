using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.SubmissionDto;

namespace EduShpere.Application.DTOs
{
    public class JuryAssignmentDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SubmissionId { get; set; }
        public string? JuryName { get; set; }
        public SubmissionResponseDto Submission { get; set; }
        public string? ScoreTemp { get; set; }
        public Dictionary<string, float>? ScoreDetail { get; set; }
        [JsonIgnore]
        public string? GradeSetting { get; set; }
        public List<string> Criteria { get; set; }
        public int TotalScore { get; set; }
    }
}
