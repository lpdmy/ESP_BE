using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShpere.Domain.Models;

public class ActivitySpeaker : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ActivityId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Title { get; set; }

    public string? Bio { get; set; }

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    public int Order { get; set; } = 0;

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("Speakers")]
    public virtual Activity Activity { get; set; } = null!;
}

