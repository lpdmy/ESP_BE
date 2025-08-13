using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ClubId", "UserId", Name = "ClubJoinRequests_index_4", IsUnique = true)]
public partial class ClubJoinRequest
{
    [Key]
    public int Id { get; set; }

    public int ClubId { get; set; }

    public int UserId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime? RespondedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ClubId")]
    [InverseProperty("ClubJoinRequests")]
    public virtual Club Club { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ClubJoinRequests")]
    public virtual User User { get; set; } = null!;
}
