using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivitySportConfigDto
{
    [Required(ErrorMessage = "Tên môn thi đấu là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tên môn thi đấu không được vượt quá 200 ký tự")]
    public string SportName { get; set; } = null!;

    [Range(1, 200, ErrorMessage = "Giới hạn thành viên phải trong khoảng 1-200")]
    public int? MaxMembers { get; set; }
}

