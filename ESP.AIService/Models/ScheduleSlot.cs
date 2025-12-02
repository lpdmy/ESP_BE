using System;

namespace ESP.AIService.Models;

/// <summary>
/// Một slot thời gian có thể được sử dụng cho match
/// </summary>
public class ScheduleSlot
{
    public DateTime MatchDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Location { get; set; }
    public float MLScore { get; set; } // Điểm dự đoán từ ML model
    public string Explanation { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
}

