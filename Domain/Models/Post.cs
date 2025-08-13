using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("UserId", "CreatedAt", Name = "IX_Posts_User_CreatedAt")]
public partial class Post
{
    [Key]
    public int Id { get; set; }

    [StringLength(500)]
    public string? Title { get; set; }

    public string? Body { get; set; }

    public int UserId { get; set; }

    [StringLength(30)]
    public string? Type { get; set; }

    public int? ClassGroupId { get; set; }

    public int? ClubId { get; set; }

    [StringLength(20)]
    public string? PrivacyLevel { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("Post")]
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    [ForeignKey("ClassGroupId")]
    [InverseProperty("Posts")]
    public virtual ClassGroup? ClassGroup { get; set; }

    [ForeignKey("ClubId")]
    [InverseProperty("Posts")]
    public virtual Club? Club { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();

    [InverseProperty("Post")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("Post")]
    public virtual ICollection<PostInterest> PostInterests { get; set; } = new List<PostInterest>();

    [InverseProperty("Post")]
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    [InverseProperty("Post")]
    public virtual ICollection<PostReport> PostReports { get; set; } = new List<PostReport>();

    [ForeignKey("UserId")]
    [InverseProperty("Posts")]
    public virtual User User { get; set; } = null!;
}
