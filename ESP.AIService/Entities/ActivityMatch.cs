using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ESP.AIService.Entities;

public partial class ActivityMatch : BaseEntity
{
    [Key]
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

    [StringLength(200)]
    public string? Location { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Pending;

    public int? Score1 { get; set; }

    public int? Score2 { get; set; }

    public int? WinnerClassGroupId { get; set; }

    public int Round { get; set; } = 1;

    [StringLength(50)]
    public string? RoundName { get; set; }

    public int MatchNumber { get; set; } = 1;

    public int? NextMatchId { get; set; }

    public bool IsBye { get; set; } = false;

    public bool IsPublished { get; set; } = false;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    // Navigation properties - using IDs only, no actual navigation to avoid conflicts
    // These will be loaded from database when needed
    public int? ActivityIdRef { get; set; }
    public int? SportIdRef { get; set; }
    public int? ClassGroup1IdRef { get; set; }
    public int? ClassGroup2IdRef { get; set; }
    public int? WinnerClassGroupIdRef { get; set; }
    public int? NextMatchIdRef { get; set; }
}

