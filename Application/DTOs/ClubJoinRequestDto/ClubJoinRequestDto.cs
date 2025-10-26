using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class ClubJoinRequestDto
    {
        public int Id { get; set; }

        public int ClubId { get; set; }
        public int UserId { get; set; }
        public string? ReasonToJoin { get; set; }
        public string? Experience { get; set; }
        public string? UserFullName { get; set; }
        public string? StudentCode { get; set; }
        public string? Status { get; set; }
        public string? Avatar { get; set; }

        public DateTime? RespondedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ClubName { get; set; }
    }
}
