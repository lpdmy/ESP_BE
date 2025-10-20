using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class UpdateCommentDto
    {
        public string? Content { get; set; }
        public int CommentId { get; set; }
    }
}
