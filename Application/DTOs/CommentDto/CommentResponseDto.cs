using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Application.DTOs
{
    public class CommentResponseDto
    {
        public int Id { get; set; }

        public int? PostId { get; set; }

        public int? ParentCommentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? UserAvatar { get; set; }
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
