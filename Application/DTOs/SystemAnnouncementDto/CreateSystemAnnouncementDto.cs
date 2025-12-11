using System.ComponentModel.DataAnnotations;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application.DTOs.SystemAnnouncementDto;

public class CreateSystemAnnouncementDto
{
    [Required(ErrorMessage = ErrorMessages.Validation.TitleRequired)]
    [StringLength(500, ErrorMessage = ErrorMessages.Validation.TitleTooLong)]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = ErrorMessages.Validation.ContentRequired)]
    public string Content { get; set; } = null!;

    [Required(ErrorMessage = ErrorMessages.Validation.AnnouncementTypeRequired)]
    [StringLength(50, ErrorMessage = ErrorMessages.Validation.AnnouncementTypeTooLong)]
    public string AnnouncementType { get; set; } = null!;

    public bool IsUrgent { get; set; } = false;

    public DateTime? ExpiryDate { get; set; }

    public List<IFormFile>? Files { get; set; }
}
