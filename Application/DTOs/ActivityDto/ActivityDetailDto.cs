using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivityDetailDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    
    // For SportsFestival
    [StringLength(50, ErrorMessage = "Hình thức thi đấu không được vượt quá 50 ký tự")]
    public string? CompetitionType { get; set; } // Individual, Team, Mixed
    
    // For CreativeContest
    [StringLength(500, ErrorMessage = "Chủ đề không được vượt quá 500 ký tự")]
    public string? Theme { get; set; }
    
    [StringLength(200, ErrorMessage = "Loại hình sáng tạo không được vượt quá 200 ký tự")]
    public string? Genre { get; set; }
    
    [StringLength(200, ErrorMessage = "Kích thước/Độ dài không được vượt quá 200 ký tự")]
    public string? PaperSize { get; set; }
    
    [StringLength(200, ErrorMessage = "Chất liệu/Thể loại không được vượt quá 200 ký tự")]
    public string? DrawingMedium { get; set; }
    
    [StringLength(200, ErrorMessage = "Thời gian làm bài không được vượt quá 200 ký tự")]
    public string? TimeLimit { get; set; }
    
    [StringLength(500, ErrorMessage = "Format nộp bài không được vượt quá 500 ký tự")]
    public string? SubmissionFormat { get; set; }
}

