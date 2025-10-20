using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class ClubMemberResponseDto
    {
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public int UserId { get; set; }
        public string CategoryName { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }
    }
}
