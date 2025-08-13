using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("CommentId", "UserId", Name = "CommentLikes_index_7", IsUnique = true)]
public partial class CommentLike
{
    [Key]
    public int Id { get; set; }

    public int CommentId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("CommentId")]
    [InverseProperty("CommentLikes")]
    public virtual Comment Comment { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("CommentLikes")]
    public virtual User User { get; set; } = null!;
}
