using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[PrimaryKey("FollowingUserId", "FollowedUserId")]
public partial class Follow
{
    [Key]
    public int FollowingUserId { get; set; }

    [Key]
    public int FollowedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("FollowedUserId")]
    [InverseProperty("FollowFollowedUsers")]
    public virtual User FollowedUser { get; set; } = null!;

    [ForeignKey("FollowingUserId")]
    [InverseProperty("FollowFollowingUsers")]
    public virtual User FollowingUser { get; set; } = null!;
}
