using System;
using System.Collections.Generic;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// Response DTO cho lịch thi đấu đã được tạo
/// </summary>
public class GenerateTournamentScheduleResponseDto
{
    public bool Success { get; set; }
    public bool IsOptimal { get; set; }
    public List<AIActivityMatchDto> GeneratedMatches { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public float ObjectiveValue { get; set; }
    public int TotalMatches { get; set; }
    public int TotalRounds { get; set; }
}

/// <summary>
/// DTO cho ActivityMatch trong response
/// </summary>
public class AIActivityMatchDto
{
    public int ActivityId { get; set; }
    public int SportId { get; set; }
    public int? ClassGroup1Id { get; set; }
    public int? ClassGroup2Id { get; set; }
    public int? Grade { get; set; }
    public DateTime? MatchDate { get; set; }
    public string? StartTime { get; set; } // Format: "HH:mm"
    public string? EndTime { get; set; } // Format: "HH:mm"
    public string? Location { get; set; }
    public int Status { get; set; } // MatchStatus enum value
    public int Round { get; set; }
    public string? RoundName { get; set; }
    public int MatchNumber { get; set; }
    public int? NextMatchId { get; set; }
    public bool IsBye { get; set; }
    public string? Notes { get; set; }
}

public class ApplyTournamentScheduleRequestDto
{
    public bool IsPublished { get; set; }
    public List<ApplyTournamentMatchDto> Matches { get; set; } = new();
}

public class ApplyTournamentMatchDto
{
    public int? Id { get; set; }
    public int SportId { get; set; }
    public int? ClassGroup1Id { get; set; }
    public int? ClassGroup2Id { get; set; }
    public int? Grade { get; set; }
    public DateTime? MatchDate { get; set; }
    public string? StartTime { get; set; } // "HH:mm"
    public string? EndTime { get; set; }   // "HH:mm"
    public string? Location { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Pending;
    public int? Score1 { get; set; }
    public int? Score2 { get; set; }
    public int? WinnerClassGroupId { get; set; }
    public int Round { get; set; } = 1;
    public string? RoundName { get; set; }
    public int MatchNumber { get; set; }
    public int? NextMatchNumber { get; set; }
    public bool IsBye { get; set; }
    public string? Notes { get; set; }
}

public class ApplyTournamentScheduleResponseDto
{
    public bool Success { get; set; }
    public bool IsPublished { get; set; }
    public int TotalMatchesApplied { get; set; }
    public List<int> MatchIds { get; set; } = new();
    public List<ScheduleConflictDto> Conflicts { get; set; } = new();
}

public class ScheduleConflictDto
{
    public int MatchNumber { get; set; }
    public string ConflictType { get; set; } = string.Empty; // "ActivityConflict" | "ClassScheduleConflict"
    public int? ActivityId { get; set; }
    public int? UserId { get; set; }
    public int? ClassGroupId { get; set; }
    public DateTime MatchDate { get; set; }
    public string? StartTime { get; set; } // "HH:mm"
    public string? EndTime { get; set; }   // "HH:mm"
    public string Message { get; set; } = string.Empty;
}

