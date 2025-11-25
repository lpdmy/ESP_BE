using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

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

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("ActivityMatches")]
    public virtual Activity Activity { get; set; } = null!;

    [ForeignKey("SportId")]
    [InverseProperty("ActivityMatches")]
    public virtual ActivitySport Sport { get; set; } = null!;

    [ForeignKey("ClassGroup1Id")]
    [InverseProperty("ActivityMatchesAsClassGroup1")]
    public virtual ClassGroup? ClassGroup1 { get; set; }

    [ForeignKey("ClassGroup2Id")]
    [InverseProperty("ActivityMatchesAsClassGroup2")]
    public virtual ClassGroup? ClassGroup2 { get; set; }

    [ForeignKey("WinnerClassGroupId")]
    [InverseProperty("ActivityMatchesAsWinner")]
    public virtual ClassGroup? WinnerClassGroup { get; set; }

    [ForeignKey("NextMatchId")]
    [InverseProperty("PreviousMatches")]
    public virtual ActivityMatch? NextMatch { get; set; }

    [InverseProperty("NextMatch")]
    public virtual ICollection<ActivityMatch> PreviousMatches { get; set; } = new List<ActivityMatch>();
}

