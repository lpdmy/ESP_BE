using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace EduShpere.Domain.Models
{
    public class Hashtag : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [InverseProperty("Hashtag")]
        public virtual ICollection<PostHashtag> PostHashtags { get; set; } = new List<PostHashtag>();
    }

}
