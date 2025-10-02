using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Application.DTOs
{
    public class CreateClubRequestDto
    {
        public string? Description { get; set; }
        [StringLength(255)]
        public string? ShortDescription { get; set; }
        public string ClubName { get; set; }
        public int CategoryId { get; set; }

        [StringLength(1000)]
        public string? AvatarUrl { get; set; }

        [StringLength(1000)]
        public string? CoverUrl { get; set; }
        public string? Requirements { get; set; }
        public bool AllowAutoJoin { get; set; } = false;
        public bool AllowMembersToPost { get; set; } = false;
        [StringLength(255)]
        public string? ContactEmail { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }
    }
}
