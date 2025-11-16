using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivityProgramDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    
    [Required(ErrorMessage = "Tên mục chương trình là bắt buộc")]
    [StringLength(500, ErrorMessage = "Tên mục chương trình không được vượt quá 500 ký tự")]
    public string Title { get; set; } = null!;
    
    [StringLength(100, ErrorMessage = "Thời gian không được vượt quá 100 ký tự")]
    public string? Time { get; set; }
    
    public string? Description { get; set; }
    
    public int Order { get; set; } = 0;
}

