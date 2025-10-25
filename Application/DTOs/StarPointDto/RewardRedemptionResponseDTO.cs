using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.StarPointDto
{
    public class RewardRedemptionResponseDTO
    {
        public int Id { get; set; }
        public int RewardId { get; set; }
        public string RewardName { get; set; }
        public int UserId { get; set; }
        public int Quantity { get; set; }
        public int TotalPointsSpent { get; set; }
        public DateTime RedeemedAt { get; set; }
    }
}
