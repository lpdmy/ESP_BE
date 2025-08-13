using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("FromUserId", "ToUserId", Name = "UX_FriendRequests_Pair", IsUnique = true)]
public partial class FriendRequest
{
    [Key]
    public int Id { get; set; }

    public int FromUserId { get; set; }

    public int ToUserId { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime? RespondedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("FromUserId")]
    [InverseProperty("FriendRequestFromUsers")]
    public virtual User FromUser { get; set; } = null!;

    [ForeignKey("ToUserId")]
    [InverseProperty("FriendRequestToUsers")]
    public virtual User ToUser { get; set; } = null!;
}
