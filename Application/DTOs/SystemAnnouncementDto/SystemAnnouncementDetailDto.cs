using EduShpere.Application.DTOs.AttachmentDto;

namespace EduShpere.Application.DTOs.SystemAnnouncementDto;

public class SystemAnnouncementDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string AnnouncementType { get; set; } = null!;
    public bool IsUrgent { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string UpdatedBy { get; set; } = null!;
    public List<SystemAttachmentDto> Attachments { get; set; } = new List<SystemAttachmentDto>();
}
