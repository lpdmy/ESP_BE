using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("UserId", "CreatedAt", Name = "IX_Posts_User_CreatedAt")]
public partial class Post : BaseEntity
{
    [Key]
    public int Id { get; set; }
    [StringLength(500)]
    public string? Title { get; set; }
    [Required]
    public string Body { get; set; } = null!;
    public int UserId { get; set; }
    public int? ClassGroupId { get; set; }
    public int? ClubId { get; set; }

    [StringLength(20)]
    public PostVisibility PrivacyLevel { get; set; } 
    [StringLength(20)]
    public PostStatus Status { get; set; } 
    [StringLength(20)]
    public string? CallToAction { get; set; } 
    
    // System Announcement fields
    public bool IsSystemAnnouncement { get; set; } = false;
    public bool IsUrgent { get; set; } = false;
    public DateTime? ExpiryDate { get; set; }
    [StringLength(50)]
    public string? AnnouncementType { get; set; }
    
    public bool IsDeleted { get; set; } = false;
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
    [ForeignKey("UserId")]
    [InverseProperty("Posts")]
    public virtual User User { get; set; } = null!;
    [InverseProperty("Post")]
    public virtual ICollection<PostHashtag> PostHashtags { get; set; } = new List<PostHashtag>();
    
}
