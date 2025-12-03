using System;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class MatchResponseDto
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public int SportId { get; set; }
        public string? SportName { get; set; }
        public int? ClassGroup1Id { get; set; }
        public string? ClassGroup1Name { get; set; }
        public int? ClassGroup2Id { get; set; }
        public string? ClassGroup2Name { get; set; }
        public int? Grade { get; set; }
        public DateTime? MatchDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Location { get; set; }
        public MatchStatus Status { get; set; }
        public int? Score1 { get; set; }
        public int? Score2 { get; set; }
        public int? WinnerClassGroupId { get; set; }
        public string? WinnerClassGroupName { get; set; }
        public int Round { get; set; }
        public string? RoundName { get; set; }
        public int MatchNumber { get; set; }
        public int? NextMatchId { get; set; }
        public bool IsBye { get; set; }
        public string? Notes { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

