using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class UpdateMatchResultDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Điểm phải >= 0")]
        public int Score1 { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Điểm phải >= 0")]
        public int Score2 { get; set; }

        [Required]
        public int WinnerClassGroupId { get; set; }
    }
}

