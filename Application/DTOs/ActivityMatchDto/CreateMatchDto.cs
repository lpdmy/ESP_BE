using System;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class CreateMatchDto
    {
        [Required]
        public int ActivityId { get; set; }

        [Required]
        public int SportId { get; set; }

        public int? ClassGroup1Id { get; set; }

        public int? ClassGroup2Id { get; set; }

        public int? Grade { get; set; }

        public DateTime? MatchDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public int Round { get; set; } = 1;

        [StringLength(50)]
        public string? RoundName { get; set; }

        public int MatchNumber { get; set; } = 1;

        public int? NextMatchId { get; set; }

        public bool IsBye { get; set; } = false;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}

