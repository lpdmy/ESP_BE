using EduShpere.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.DTOs
{
    public class RewardWithClaimedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PointCost { get; set; }
        public int Stock { get; set; }
        public RewardCategory? Category { get; set; }
        public string ImageUrl { get; set; }
        public int Claimed { get; set; }
    }
}
