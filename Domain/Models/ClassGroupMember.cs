using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ClassGroupId", "UserId", Name = "ClassGroupMembers_index_2", IsUnique = true)]
public partial class ClassGroupMember
{
    [Key]
    public int Id { get; set; }

    public int ClassGroupId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ClassGroupId")]
    [InverseProperty("ClassGroupMembers")]
    public virtual ClassGroup ClassGroup { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ClassGroupMembers")]
    public virtual User User { get; set; } = null!;
}
