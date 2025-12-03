using System.Collections.Generic;

namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class BracketResponseDto
    {
        public int ActivityId { get; set; }
        public int SportId { get; set; }
        public string? SportName { get; set; }
        public int? Grade { get; set; }
        public int TotalRounds { get; set; }
        public int TotalMatches { get; set; }
        public List<RoundDto> Rounds { get; set; } = new List<RoundDto>();
    }

    public class RoundDto
    {
        public int RoundNumber { get; set; }
        public string? RoundName { get; set; }
        public List<MatchResponseDto> Matches { get; set; } = new List<MatchResponseDto>();
    }
}

