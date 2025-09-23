using System.ComponentModel.DataAnnotations.Schema;

namespace EduShpere.Domain.Models
{
    public class PostHashtag : BaseEntity
    {
        public int PostId { get; set; }
        public int HashtagId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("HashtagId")]
        public virtual Hashtag Hashtag { get; set; } = null!;
    }
}
