using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Submission
{
    [Key]
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public int UserId { get; set; }

    [StringLength(1000)]
    public string? FileUrl { get; set; }

    public double? Score { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public string? Comment { get; set; }

    public string? Title { get; set; }

    [ForeignKey("ActivityId")]
    [InverseProperty("Submissions")]
    public virtual Activity Activity { get; set; } = null!;

    [InverseProperty("Submission")]
    public virtual ICollection<SubmissionVote> SubmissionVotes { get; set; } = new List<SubmissionVote>();

    [ForeignKey("UserId")]
    [InverseProperty("Submissions")]
    public virtual User User { get; set; } = null!;
    public ICollection<JuryAssignment> JuryAssignments { get; set; }
}
