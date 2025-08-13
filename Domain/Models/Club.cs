using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Club
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [StringLength(1000)]
    public string? AvatarUrl { get; set; }

    [StringLength(1000)]
    public string? CoverUrl { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("Club")]
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    [InverseProperty("Club")]
    public virtual ICollection<ClubJoinRequest> ClubJoinRequests { get; set; } = new List<ClubJoinRequest>();

    [InverseProperty("Club")]
    public virtual ICollection<ClubMember> ClubMembers { get; set; } = new List<ClubMember>();

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("Clubs")]
    public virtual User CreatedByUser { get; set; } = null!;

    [InverseProperty("Club")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
