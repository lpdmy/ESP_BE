using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class ActivityParticipant : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int ActivityId { get; set; }

    public int UserId { get; set; }

    public int? ClassGroupId { get; set; }

    [StringLength(50)]
    public ParticipantStatus? Status { get; set; }

    public int? SportId { get; set; }

    public Guid? GroupCode { get; set; }

    public bool IsLeader { get; set; }

    [StringLength(2000)]
    public string? RegistrationMetadata { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("ActivityParticipants")]
    public virtual Activity Activity { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ActivityParticipants")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("ClassGroupId")]
    [InverseProperty("ActivityParticipants")]
    public virtual ClassGroup? ClassGroup { get; set; }

    [ForeignKey("SportId")]
    [InverseProperty("ActivityParticipants")]
    public virtual ActivitySport? Sport { get; set; }
}
