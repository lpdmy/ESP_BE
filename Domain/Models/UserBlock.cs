using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("BlockerId", "BlockedId", Name = "UX_UserBlocks_Pair", IsUnique = true)]
public partial class UserBlock
{
    [Key]
    public int Id { get; set; }

    public int BlockerId { get; set; }

    public int BlockedId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("BlockedId")]
    [InverseProperty("UserBlockBlockeds")]
    public virtual User Blocked { get; set; } = null!;

    [ForeignKey("BlockerId")]
    [InverseProperty("UserBlockBlockers")]
    public virtual User Blocker { get; set; } = null!;
}
