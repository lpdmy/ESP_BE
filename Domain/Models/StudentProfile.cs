using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("UserId", Name = "UQ__StudentP__1788CC4D37CE064E", IsUnique = true)]
public partial class StudentProfile
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string? StudentNumber { get; set; }

    public short? EnrollmentYear { get; set; }

    [StringLength(1000)]
    public string? Bio { get; set; }

    public string? ExtraJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("StudentProfile")]
    public virtual User User { get; set; } = null!;
}
