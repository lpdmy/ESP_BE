using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Activity
{
    [Key]
    public int Id { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }


    [StringLength(20)]
    public string? Scope { get; set; }

    public int? ClubId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public string RowVersion { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string SubCategory { get; set; } = null!;
    public string ThumbNailUrl { get; set; } = null!;

    [InverseProperty("Activity")]
    public virtual ICollection<ActivityParticipant> ActivityParticipants { get; set; } = new List<ActivityParticipant>();

    [InverseProperty("Activity")]
    public virtual ICollection<ActivityReward> ActivityRewards { get; set; } = new List<ActivityReward>();

    [ForeignKey("ClubId")]
    [InverseProperty("Activities")]
    public virtual Club? Club { get; set; }

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("Activities")]
    public virtual User CreatedByUser { get; set; } = null!;

    [InverseProperty("Activity")]
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
