using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class School
{
    [Key]
    public long SchoolId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [InverseProperty("School")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    [InverseProperty("School")]
    public virtual ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
}
