using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Models;

namespace EduShpere.Domain
{
    public partial class ClubCategory : BaseEntity  
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [InverseProperty("Category")]
        public virtual ICollection<Club> Clubs { get; set; } = new List<Club>();
    }
}
