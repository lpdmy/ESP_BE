using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShpere.Domain.Models;

public class ActivityProgram : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ActivityId { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    [StringLength(100)]
    public string? Time { get; set; }

    public string? Description { get; set; }

    public int Order { get; set; } = 0;

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("Programs")]
    public virtual Activity Activity { get; set; } = null!;
}

