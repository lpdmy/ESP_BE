using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class CreateClubDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }
        public int CategoryId { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public int CreatedByUserId { get; set; }
        public string? Requirements { get; set; }
        public bool AllowAutoJoin { get; set; } = false;
        public bool AllowMembersToPost { get; set; } = false;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public int? MentorUserId { get; set; }
        public int? PresidentUserId { get; set; }
    }
}
