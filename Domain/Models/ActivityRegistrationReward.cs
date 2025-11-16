using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ActivityId", Name = "IX_ActivityRegistrationReward_ActivityId", IsUnique = true)]
public class ActivityRegistrationReward : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ActivityId { get; set; }

    [Required]
    public int StarPoints { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("RegistrationReward")]
    public virtual Activity Activity { get; set; } = null!;
}

