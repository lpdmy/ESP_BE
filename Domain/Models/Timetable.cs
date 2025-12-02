using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Timetable : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int ClassGroupId { get; set; }

    public int DayOfWeek { get; set; } // 1=Monday, 2=Tuesday, ..., 7=Sunday

    [Column(TypeName = "time")]
    public TimeSpan StartTime { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan EndTime { get; set; }

    [StringLength(200)]
    public string? SubjectName { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [ForeignKey("ClassGroupId")]
    [InverseProperty("Timetables")]
    public virtual ClassGroup ClassGroup { get; set; } = null!;
}


