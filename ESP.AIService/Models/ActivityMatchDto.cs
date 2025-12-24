using System;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;

namespace ESP.AIService.Models;

/// <summary>
/// DTO để map từ SQL query - chỉ chứa các columns thực sự có trong database
/// </summary>
public class ActivityMatchDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public int SportId { get; set; }
    public int? ClassGroup1Id { get; set; }
    public int? ClassGroup2Id { get; set; }
    public int? Grade { get; set; }
    public DateTime? MatchDate { get; set; }
    
    [Column(TypeName = "time")]
    public TimeSpan? StartTime { get; set; }
    
    [Column(TypeName = "time")]
    public TimeSpan? EndTime { get; set; }
    
    public string? Location { get; set; }
    public MatchStatus Status { get; set; }
    public int? Score1 { get; set; }
    public int? Score2 { get; set; }
    public int? WinnerClassGroupId { get; set; }
    public int Round { get; set; }
    public string? RoundName { get; set; }
    public int MatchNumber { get; set; }
    public int? NextMatchId { get; set; }
    public bool IsBye { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    
    // Convert to ActivityMatch entity
    public ActivityMatch ToActivityMatch()
    {
        return new ActivityMatch
        {
            Id = Id,
            ActivityId = ActivityId,
            SportId = SportId,
            ClassGroup1Id = ClassGroup1Id,
            ClassGroup2Id = ClassGroup2Id,
            Grade = Grade,
            MatchDate = MatchDate,
            StartTime = StartTime,
            EndTime = EndTime,
            Location = Location,
            Status = Status,
            Score1 = Score1,
            Score2 = Score2,
            WinnerClassGroupId = WinnerClassGroupId,
            Round = Round,
            RoundName = RoundName,
            MatchNumber = MatchNumber,
            NextMatchId = NextMatchId,
            IsBye = IsBye,
            Notes = Notes,
            CreatedAt = CreatedAt,
            CreatedBy = CreatedBy,
            UpdatedAt = UpdatedAt,
            UpdatedBy = UpdatedBy,
            IsDeleted = IsDeleted,
            RowVersion = new byte[8] // Dummy, không cần thiết cho query
        };
    }
}

