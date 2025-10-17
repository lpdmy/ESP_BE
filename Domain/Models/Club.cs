using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Club : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    public string? Description { get; set; }
    [StringLength(255)]
    public string? ShortDescription { get; set; } // Mô tả ngắn
    public int CategoryId { get; set; } // Danh mục

    [StringLength(1000)]
    public string? AvatarUrl { get; set; }

    [StringLength(1000)]
    public string? CoverUrl { get; set; }

    public int CreatedByUserId { get; set; }
    public string? Requirements { get; set; }
    public bool AllowAutoJoin { get; set; } = false;
    public bool AllowMembersToPost { get; set; } = false;
    [StringLength(255)]
    public string? ContactEmail { get; set; }

    [StringLength(20)]
    public string? ContactPhone { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public int? MentorUserId { get; set; } // Người hướng dẫn
    public int? PresidentUserId { get; set; } // Chủ tịch CLB


    [InverseProperty("Club")]
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

    [InverseProperty("Club")]
    public virtual ICollection<ClubJoinRequest> ClubJoinRequests { get; set; } = new List<ClubJoinRequest>();

    [InverseProperty("Club")]
    public virtual ICollection<ClubMember> ClubMembers { get; set; } = new List<ClubMember>();

    [ForeignKey("CreatedByUserId")]
    [InverseProperty("Clubs")]
    public virtual User CreatedByUser { get; set; } = null!;
    [ForeignKey("CategoryId")]
    [InverseProperty("Clubs")]
    public virtual ClubCategory Category { get; set; } = null!;

    [InverseProperty("Club")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    [ForeignKey("MentorUserId")]
    [InverseProperty("MentoredClubs")]
    public virtual User? Mentor { get; set; }

    [ForeignKey("PresidentUserId")]
    [InverseProperty("PresidedClubs")]
    public virtual User? President { get; set; }
}
