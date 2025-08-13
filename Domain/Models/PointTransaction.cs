using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("UserId", "CreatedAt", Name = "PointTransactions_index_12")]
public partial class PointTransaction
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int Change { get; set; }

    [StringLength(255)]
    public string? Reason { get; set; }

    [StringLength(50)]
    public string? SourceType { get; set; }

    public int? SourceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PointTransactions")]
    public virtual User User { get; set; } = null!;
}
