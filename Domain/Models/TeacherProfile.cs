using EduShpere.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Domain.Models
{
    public class TeacherProfile : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(100)]
        public string? TeacherCode { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [StringLength(1000)]
        public string? ExtraJson { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}