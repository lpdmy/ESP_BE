using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class PostReport
{
    [Key]
    public int Id { get; set; }

    public int PostId { get; set; }

    public int ReporterId { get; set; }

    [StringLength(255)]
    public string? Reason { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("PostId")]
    [InverseProperty("PostReports")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("ReporterId")]
    [InverseProperty("PostReports")]
    public virtual User Reporter { get; set; } = null!;
}
