using EduShpere.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class RewardRule
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public RewardActionType ActionType { get; set; }

        [Required]
        public int Points { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit fields
        public int CreatedBy { get; set; } = 21;

        public int? UpdatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
