using EduShpere.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.StarPointDto
{
    public class RewardDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Range(0, int.MaxValue)]
        public int PointCost { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public RewardCategory Category { get; set; } = RewardCategory.Voucher;

        [MaxLength(500)]
        public string ImageUrl { get; set; }
    }
}
