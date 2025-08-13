using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("PostId", "UserId", Name = "PostInterests_index_8", IsUnique = true)]
public partial class PostInterest
{
    [Key]
    public int Id { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    [StringLength(20)]
    public string? InterestStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("PostId")]
    [InverseProperty("PostInterests")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("PostInterests")]
    public virtual User User { get; set; } = null!;
}
