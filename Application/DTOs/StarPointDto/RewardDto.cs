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

        /// <summary>
        /// Cho phép null để tránh lỗi ModelState khi FE gửi chuỗi rỗng.
        /// Nếu null thì backend sẽ tự gán mặc định là Voucher.
        /// </summary>
        public RewardCategory? Category { get; set; } = RewardCategory.Voucher;

        [MaxLength(500)]
        public string ImageUrl { get; set; }
    }
}
