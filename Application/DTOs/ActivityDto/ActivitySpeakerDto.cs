using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivitySpeakerDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    
    [Required(ErrorMessage = "Tên diễn giả là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên diễn giả không được vượt quá 200 ký tự")]
    public string Name { get; set; } = null!;
    
    [StringLength(200, ErrorMessage = "Chức danh không được vượt quá 200 ký tự")]
    public string? Title { get; set; }
    
    public string? Bio { get; set; }
    
    [StringLength(1000, ErrorMessage = "URL ảnh không được vượt quá 1000 ký tự")]
    public string? ImageUrl { get; set; }
    
    public int Order { get; set; } = 0;
}

