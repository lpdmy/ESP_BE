using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("CollectionId", "PostId", Name = "CollectionItems_index_9", IsUnique = true)]
public partial class CollectionItem
{
    [Key]
    public int Id { get; set; }

    public int CollectionId { get; set; }

    public int PostId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("CollectionId")]
    [InverseProperty("CollectionItems")]
    public virtual FavoriteCollection Collection { get; set; } = null!;

    [ForeignKey("PostId")]
    [InverseProperty("CollectionItems")]
    public virtual Post Post { get; set; } = null!;
}
