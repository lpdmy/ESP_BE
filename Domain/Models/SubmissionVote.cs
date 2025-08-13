using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("SubmissionId", "UserId", Name = "SubmissionVotes_index_11", IsUnique = true)]
public partial class SubmissionVote
{
    [Key]
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("SubmissionId")]
    [InverseProperty("SubmissionVotes")]
    public virtual Submission Submission { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("SubmissionVotes")]
    public virtual User User { get; set; } = null!;
}
