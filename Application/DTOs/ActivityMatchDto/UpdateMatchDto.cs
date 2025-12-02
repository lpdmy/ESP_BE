using System;
using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class UpdateMatchDto
    {
        public int? ClassGroup1Id { get; set; }

        public int? ClassGroup2Id { get; set; }

        public DateTime? MatchDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        public MatchStatus? Status { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}

