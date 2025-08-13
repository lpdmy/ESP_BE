using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("UserId", Name = "UQ__TeacherP__1788CC4D4A59778B", IsUnique = true)]
public partial class TeacherProfile
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(500)]
    public string? SubjectSpecialties { get; set; }

    [StringLength(100)]
    public string? Title { get; set; }

    public string? ExtraJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("TeacherProfile")]
    public virtual User User { get; set; } = null!;
}
