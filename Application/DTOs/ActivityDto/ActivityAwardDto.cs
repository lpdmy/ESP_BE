using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivityAwardDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    
    [StringLength(50, ErrorMessage = "Tên giải không được vượt quá 50 ký tự")]
    public string? Name { get; set; }
    
    [StringLength(50, ErrorMessage = "Tên giải không được vượt quá 50 ký tự")]
    public string? Rank { get; set; } // Alias for Name, maps to ActivityReward.Rank
    
    [Required(ErrorMessage = "Điểm thưởng là bắt buộc")]
    [Range(0, int.MaxValue, ErrorMessage = "Điểm thưởng phải lớn hơn hoặc bằng 0")]
    public int StarPoints { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Điểm thưởng phải lớn hơn hoặc bằng 0")]
    public int Points { get; set; } // Alias for StarPoints, maps to ActivityReward.StarPoints
}

