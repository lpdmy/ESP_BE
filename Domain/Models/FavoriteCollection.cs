using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class FavoriteCollection : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("Collection")]
    public virtual ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();

    [ForeignKey("UserId")]
    [InverseProperty("FavoriteCollections")]
    public virtual User User { get; set; } = null!;
}
