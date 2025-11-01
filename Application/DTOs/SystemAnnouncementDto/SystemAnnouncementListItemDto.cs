using EduShpere.Application.DTOs;

namespace EduShpere.Application.DTOs.SystemAnnouncementDto;

public class SystemAnnouncementListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string AnnouncementType { get; set; } = null!;
    public bool IsUrgent { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string CreatedBy { get; set; } = null!;
    public int AttachmentCount { get; set; }
}
