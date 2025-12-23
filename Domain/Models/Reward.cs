using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public class Reward
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; }

    public int PointCost { get; set; }

    public int Stock { get; set; }

    public RewardCategory? Category { get; set; } = RewardCategory.Voucher;

    [MaxLength(500)]
    public string ImageUrl { get; set; }

    // Audit fields
    public int CreatedBy { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
    public bool TypeRequiresPickup()
    {
        return Category != RewardCategory.Avatar && Category != RewardCategory.Theme;
    }
}
