using System;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ClassGroupDto;

public class ClassGroupScheduleDto
{
    public int Id { get; set; }
    public int ClassGroupId { get; set; }
    
    /// <summary>
    /// Thứ trong tuần: 1 = Monday, 2 = Tuesday, ..., 7 = Sunday
    /// </summary>
    public int DayOfWeek { get; set; }
    
    /// <summary>
    /// Tiết học: 1, 2, 3, ...
    /// </summary>
    public int Period { get; set; }
    
    /// <summary>
    /// Giờ bắt đầu (ví dụ: "07:00")
    /// </summary>
    public TimeSpan StartTime { get; set; }
    
    /// <summary>
    /// Giờ kết thúc (ví dụ: "07:45")
    /// </summary>
    public TimeSpan EndTime { get; set; }
    
    /// <summary>
    /// Môn học (optional)
    /// </summary>
    public string? Subject { get; set; }
}

