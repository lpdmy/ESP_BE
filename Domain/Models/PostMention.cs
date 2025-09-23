using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class PostMention : BaseEntity
    {
        public int PostId { get; set; }
        public int MentionedUserId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("MentionedUserId")]
        public virtual User MentionedUser { get; set; } = null!;
    }
}
