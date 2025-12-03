using System;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ClassGroupDto;

public class CreateClassGroupScheduleDto
{
    [Required]
    [Range(1, 7, ErrorMessage = "DayOfWeek phải từ 1 (Thứ 2) đến 7 (Chủ nhật)")]
    public int DayOfWeek { get; set; }

    [Required]
    [Range(1, 12, ErrorMessage = "Period phải từ 1 đến 12")]
    public int Period { get; set; }

    /// <summary>
    /// Giờ bắt đầu (format: "HH:mm" hoặc "HH:mm:ss")
    /// </summary>
    [Required]
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// Giờ kết thúc (format: "HH:mm" hoặc "HH:mm:ss")
    /// </summary>
    [Required]
    public string EndTime { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Subject { get; set; }
}

