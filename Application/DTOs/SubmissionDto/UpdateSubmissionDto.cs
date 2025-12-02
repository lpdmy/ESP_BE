using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.SubmissionDto
{
    public class UpdateSubmissionDto
    {
        [Required(ErrorMessage = "Id là bắt buộc")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title là bắt buộc")]
        [StringLength(500, ErrorMessage = "Title không được vượt quá 500 ký tự")]
        public string Title { get; set; } = null!;

        public List<SubmissionAttachmentDto> Attachments { get; set; } = new List<SubmissionAttachmentDto>();
    }
}

