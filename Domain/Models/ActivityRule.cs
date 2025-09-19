using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class ActivityRule : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public string RuleText { get; set; } = null!;

        [ForeignKey("ActivityId")]
        [InverseProperty("Rules")]
        public virtual Activity Activity { get; set; } = null!;
    }
}
