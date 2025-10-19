using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public class RewardRedemption
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Reward")]
    public int RewardId { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    public int Quantity { get; set; } = 1;

    public int TotalPointsSpent { get; set; }

    // Navigation properties
    public Reward Reward { get; set; }

    public User User { get; set; }

    // Audit fields
    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
    public RedemptionStatus Status { get; set; } = RedemptionStatus.Pending;

}