using System.Collections.Generic;

namespace ESP.AIService.Models;

/// <summary>
/// Giải thích logic AI khi chọn slot cho một match
/// </summary>
public class ScheduleExplanation
{
    public int MatchId { get; set; }
    public string? MatchDescription { get; set; } // "9A vs 9B"
    
    public ChosenSlotInfo? ChosenSlot { get; set; }
    
    public List<string> ConstraintsApplied { get; set; } = new(); // ["Ưu tiên lớp cùng lịch học", "Tránh giờ trùng timetable"]
    
    public List<ConflictInfo> Conflicts { get; set; } = new(); // Conflicts đã tránh
    
    public List<AlternativeSlotInfo> AlternativesTried { get; set; } = new(); // Các slot đã thử nhưng bị loại
    
    public float Score { get; set; } // Score của slot đã chọn
}

public class ChosenSlotInfo
{
    public string Date { get; set; } = string.Empty; // "2024-04-25"
    public string StartTime { get; set; } = string.Empty; // "14:00"
    public string EndTime { get; set; } = string.Empty; // "15:30"
    public string? Location { get; set; }
    public string Reason { get; set; } = string.Empty; // "Slot phù hợp với tất cả các lớp"
}

public class ConflictInfo
{
    public string Type { get; set; } = string.Empty; // "Timetable", "ParticipantConflict", "ActivityConflict"
    public string? Class { get; set; } // "9A"
    public string? User { get; set; } // "Nguyễn Văn A"
    public int? ActivityId { get; set; }
    public string? ActivityTitle { get; set; }
    public string Detail { get; set; } = string.Empty; // "9A học Văn 13:00-14:00"
}

public class AlternativeSlotInfo
{
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string Reason { get; set; } = string.Empty; // "Conflict với lịch học lớp 9A"
    public float Score { get; set; }
}


