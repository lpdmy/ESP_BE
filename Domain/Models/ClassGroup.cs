using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class ClassGroup
{
    [Key]
    public int Id { get; set; }

    [StringLength(255)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("ClassGroup")]
    public virtual ICollection<ClassGroupMember> ClassGroupMembers { get; set; } = new List<ClassGroupMember>();

    [InverseProperty("ClassGroup")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
