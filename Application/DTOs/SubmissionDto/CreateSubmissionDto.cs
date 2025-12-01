using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.SubmissionDto
{
    public class CreateSubmissionDto
    {
        [Required(ErrorMessage = "ActivityId là bắt buộc")]
        public int ActivityId { get; set; }
        
        [Required(ErrorMessage = "FileUrl là bắt buộc")]
        [StringLength(1000, ErrorMessage = "FileUrl không được vượt quá 1000 ký tự")]
        public string FileUrl { get; set; } = null!;
        
        [StringLength(500, ErrorMessage = "Title không được vượt quá 500 ký tự")]
        public string? Title { get; set; }
    }
}


