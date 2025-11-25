using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// Request DTO để tạo lịch thi đấu bằng AI
/// </summary>
public class GenerateTournamentScheduleRequestDto
{
    [Required(ErrorMessage = "ActivityId là bắt buộc")]
    public int ActivityId { get; set; }

    [Required(ErrorMessage = "SportId là bắt buộc")]
    public int SportId { get; set; }

    [Required(ErrorMessage = "Cần ít nhất 2 class groups")]
    [MinLength(2, ErrorMessage = "Cần ít nhất 2 class groups để tạo lịch thi đấu")]
    public List<int> ClassGroupIds { get; set; } = new();

    public int? Grade { get; set; }

    [Required(ErrorMessage = "StartDate là bắt buộc")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "EndDate là bắt buộc")]
    public DateTime EndDate { get; set; }

    public TimeSpan MatchDuration { get; set; } = TimeSpan.FromHours(1); // Default 1 hour

    public TimeSpan? PreferredStartTime { get; set; } // Giờ bắt đầu ưu tiên (ví dụ: 8:00)

    public TimeSpan? PreferredEndTime { get; set; } // Giờ kết thúc ưu tiên (ví dụ: 17:00)

    public List<string> AvailableLocations { get; set; } = new();

    [Range(1, 50, ErrorMessage = "MaxMatchesPerDay phải từ 1 đến 50")]
    public int MaxMatchesPerDay { get; set; } = 10;

    [Range(0, 120, ErrorMessage = "MinGapBetweenMatches phải từ 0 đến 120 phút")]
    public int MinGapBetweenMatches { get; set; } = 30; // Phút

    public string TournamentFormat { get; set; } = "SingleElimination"; // SingleElimination, RoundRobin, DoubleElimination

    /// <summary>
    /// Ghi chú từ người dùng để AI đánh giá và chọn slot phù hợp
    /// Ví dụ: "Ưu tiên buổi sáng", "Tránh giờ cao điểm", "Cuối tuần tốt hơn"
    /// </summary>
    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
    public string? UserNotes { get; set; }
}

