using System;

namespace ESP.AIService.Models;

/// <summary>
/// Request để tạo lịch thi đấu
/// </summary>
public class TournamentScheduleRequest
{
    public int ActivityId { get; set; }
    public int SportId { get; set; }
    public List<int> ClassGroupIds { get; set; } = new();
    public int? Grade { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan MatchDuration { get; set; } = TimeSpan.FromHours(1); // Default 1 hour
    public TimeSpan? PreferredStartTime { get; set; } // Giờ bắt đầu ưu tiên (ví dụ: 8:00)
    public TimeSpan? PreferredEndTime { get; set; } // Giờ kết thúc ưu tiên (ví dụ: 17:00)
    public List<string> AvailableLocations { get; set; } = new();
    public int MaxMatchesPerDay { get; set; } = 10;
    public int MinGapBetweenMatches { get; set; } = 30; // Phút
    public string TournamentFormat { get; set; } = "SingleElimination"; // SingleElimination, RoundRobin, DoubleElimination
    public string? UserNotes { get; set; } // Ghi chú từ người dùng để AI đánh giá slot
}

