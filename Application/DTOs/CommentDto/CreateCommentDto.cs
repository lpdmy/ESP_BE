using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class CreateCommentDto
    {
        [JsonPropertyName("postId")]
        public int? PostId { get; set; }
        [JsonPropertyName("parentCommentId")]
        public int? ParentCommentId { get; set; }
        [JsonPropertyName("content")]
        public string? Content { get; set; }

    }
}
