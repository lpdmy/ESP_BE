using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class GenerateBracketDto
    {
        [Required]
        public int ActivityId { get; set; }

        [Required]
        public int SportId { get; set; }

        public int? Grade { get; set; } // Nếu null thì generate cho tất cả khối
    }
}

