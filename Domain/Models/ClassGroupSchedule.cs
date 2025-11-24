using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

/// <summary>
/// Lịch học của lớp (thứ trong tuần, tiết học)
/// </summary>
[Index("ClassGroupId", "DayOfWeek", "Period", Name = "IX_ClassGroupSchedule_ClassGroup_Day_Period", IsUnique = true)]
public partial class ClassGroupSchedule : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ClassGroupId { get; set; }

    /// <summary>
    /// Thứ trong tuần: 1 = Monday, 2 = Tuesday, ..., 7 = Sunday
    /// </summary>
    [Required]
    [Range(1, 7, ErrorMessage = "DayOfWeek phải từ 1 (Thứ 2) đến 7 (Chủ nhật)")]
    public int DayOfWeek { get; set; }

    /// <summary>
    /// Tiết học: 1, 2, 3, ...
    /// </summary>
    [Required]
    [Range(1, 12, ErrorMessage = "Period phải từ 1 đến 12")]
    public int Period { get; set; }

    /// <summary>
    /// Giờ bắt đầu (ví dụ: 07:00)
    /// </summary>
    [Required]
    [Column(TypeName = "time")]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Giờ kết thúc (ví dụ: 07:45)
    /// </summary>
    [Required]
    [Column(TypeName = "time")]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Môn học (optional)
    /// </summary>
    [StringLength(200)]
    public string? Subject { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ClassGroupId")]
    [InverseProperty("Schedules")]
    public virtual ClassGroup ClassGroup { get; set; } = null!;
}

