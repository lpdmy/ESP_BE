using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ActivityId", "UserId", Name = "ActivityParticipants_index_10", IsUnique = true)]
public partial class ActivityParticipant
{
    [Key]
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public int UserId { get; set; }

    [StringLength(50)]
    public ParticipantStatus? Status { get; set; }
    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("ActivityParticipants")]
    public virtual Activity Activity { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ActivityParticipants")]
    public virtual User User { get; set; } = null!;
}
