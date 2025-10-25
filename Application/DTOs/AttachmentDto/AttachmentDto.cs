namespace EduShpere.Application.DTOs.AttachmentDto;

public class SystemAttachmentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string? FileUrl { get; set; }
    public string? FileType { get; set; }
    public string? Mime { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTime CreatedAt { get; set; }
}
