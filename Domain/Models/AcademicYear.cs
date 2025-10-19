using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class AcademicYear
    {
        public int Id { get; set; }
        public long SchoolId { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public bool? IsCurrent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public byte[] RowVersion { get; set; }

        [InverseProperty("AcademicYears")]
        public virtual ICollection<ClassGroup> ClassGroups { get; set; } = new List<ClassGroup>();

        [InverseProperty("AcademicYears")]
        public virtual School School { get; set; } = null!;
    }
}
