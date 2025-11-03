using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class ClubMemberDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserRole { get; set; }
    }
}
