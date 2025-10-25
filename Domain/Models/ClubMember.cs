using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ClubId", "UserId", Name = "ClubMembers_index_3", IsUnique = true)]
public partial class ClubMember : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int ClubId { get; set; }

    public int UserId { get; set; }

    [StringLength(50)]
    public string? Role { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ClubId")]
    [InverseProperty("ClubMembers")]
    public virtual Club Club { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
