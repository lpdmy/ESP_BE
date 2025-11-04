using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ClubId", "UserId", Name = "ClubJoinRequests_index_4", IsUnique = true)]
public partial class ClubJoinRequest : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int ClubId { get; set; }
    [StringLength(255)]
    public int UserId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime? RespondedAt { get; set; }
    public string? ReasonToJoin { get; set; }
    public string ? Experience { get; set; }
    public bool? IsMentor { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ClubId")]
    [InverseProperty("ClubJoinRequests")]
    public virtual Club Club { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ClubJoinRequests")]
    public virtual User User { get; set; } = null!;
}
