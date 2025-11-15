using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivityRegistrationRewardDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    
    [Required(ErrorMessage = "Điểm thưởng là bắt buộc")]
    [Range(0, int.MaxValue, ErrorMessage = "Điểm thưởng phải lớn hơn hoặc bằng 0")]
    public int StarPoints { get; set; }
}

