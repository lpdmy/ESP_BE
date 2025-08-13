using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class ActivityReward
{
    [Key]
    public int Id { get; set; }

    public int ActivityId { get; set; }

    [StringLength(50)]
    public string? Rank { get; set; }

    public int StarPoints { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("ActivityRewards")]
    public virtual Activity Activity { get; set; } = null!;
}
