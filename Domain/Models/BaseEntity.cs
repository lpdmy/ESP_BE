using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduShpere.Domain.Models
{
    public abstract class BaseEntity
    {
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; } = false;

    }
}
