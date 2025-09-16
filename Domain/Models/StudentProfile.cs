using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShpere.Domain.Models
{
    public partial class StudentProfile : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [StringLength(100)]
        public string? StudentNumber { get; set; }

        public short? EnrollmentYear { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        public string? ExtraJson { get; set; }


        public byte[] RowVersion { get; set; } = null!;

        [ForeignKey("UserId")]
        [InverseProperty("StudentProfile")]
        public virtual User User { get; set; } = null!;
    }
}
