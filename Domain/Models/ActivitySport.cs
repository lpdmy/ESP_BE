using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShpere.Domain.Models;

public class ActivitySport : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ActivityId { get; set; }

    [Required]
    [StringLength(200)]
    public string SportName { get; set; } = null!;

    public bool IsCustom { get; set; } = false;

    public int? MaxMembers { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("Sports")]
    public virtual Activity Activity { get; set; } = null!;

    [InverseProperty("Sport")]
    public virtual ICollection<ActivityMatch> ActivityMatches { get; set; } = new List<ActivityMatch>();

    [InverseProperty("Sport")]
    public virtual ICollection<ActivityParticipant> ActivityParticipants { get; set; } = new List<ActivityParticipant>();
}

