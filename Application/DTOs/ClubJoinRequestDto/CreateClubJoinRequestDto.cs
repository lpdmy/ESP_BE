using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class CreateClubJoinRequestDto
    {
        public int ClubId { get; set; }
        public string? ReasonToJoin { get; set; }
        public string? Experience { get; set; }
    }
}
