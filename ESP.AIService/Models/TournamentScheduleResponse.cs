using ESP.AIService.Entities;
using System.Collections.Generic;

namespace ESP.AIService.Models;

/// <summary>
/// Response chứa lịch thi đấu đã được tạo
/// </summary>
public class TournamentScheduleResponse
{
    public bool Success { get; set; }
    public bool IsOptimal { get; set; }
    public List<ActivityMatch> GeneratedMatches { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public float ObjectiveValue { get; set; }
    public int TotalMatches { get; set; }
    public int TotalRounds { get; set; }
}

