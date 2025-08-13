using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class RewardRedemptionLog
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RewardId { get; set; }

    public int PointsSpent { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("RewardId")]
    [InverseProperty("RewardRedemptionLogs")]
    public virtual Reward Reward { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("RewardRedemptionLogs")]
    public virtual User User { get; set; } = null!;
}
