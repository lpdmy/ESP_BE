using EduShpere.Domain.Models;
using System.Collections.Generic;

namespace ESP.AIService.Models;

/// <summary>
/// Response chứa lịch thi đấu đã được tạo
/// </summary>
public class TournamentScheduleResponse
{
    public bool Success { get; set; }
    public bool IsOptimal { get; set; }
    public List<ActivityMatch> GeneratedMatches { get; set; } = new();
    public string Explanation { get; set; } = string.Empty;
    public float ObjectiveValue { get; set; }
    public int TotalMatches { get; set; }
    public int TotalRounds { get; set; }
    
    /// <summary>
    /// Danh sách explanations cho từng match - giải thích logic AI
    /// </summary>
    public List<ScheduleExplanation> Explanations { get; set; } = new();
    
    /// <summary>
    /// Danh sách conflicts đã phát hiện và tránh
    /// </summary>
    public List<ConflictInfo> DetectedConflicts { get; set; } = new();
    
    /// <summary>
    /// Danh sách lý do không đủ slots - hiển thị cho người dùng
    /// </summary>
    public List<SlotWarningReason> SlotWarnings { get; set; } = new();
    
    /// <summary>
    /// Danh sách chi tiết các conflicts khiến slots bị loại bỏ
    /// </summary>
    public List<SlotConflictDetail> SlotConflicts { get; set; } = new();
}

/// <summary>
/// Lý do không đủ slots - hiển thị chi tiết cho người dùng
/// </summary>
public class SlotWarningReason
{
    /// <summary>
    /// Loại cảnh báo: "InsufficientSlots", "TooManyConflicts", "TimeRangeTooNarrow", etc.
    /// </summary>
    public string WarningType { get; set; } = string.Empty;
    
    /// <summary>
    /// Tiêu đề ngắn gọn
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Mô tả chi tiết
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Giá trị hiện tại
    /// </summary>
    public string CurrentValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Giá trị khuyến nghị
    /// </summary>
    public string RecommendedValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Giải pháp đề xuất
    /// </summary>
    public string Solution { get; set; } = string.Empty;
    
    /// <summary>
    /// Mức độ nghiêm trọng: "Low", "Medium", "High", "Critical"
    /// </summary>
    public string Severity { get; set; } = "Medium";
    
    /// <summary>
    /// Các trường cần kiểm tra (danh sách field names, phân cách bằng dấu phẩy)
    /// </summary>
    public string Field { get; set; } = string.Empty;
}

/// <summary>
/// Chi tiết conflict khiến một slot bị loại bỏ
/// </summary>
public class SlotConflictDetail
{
    /// <summary>
    /// Thông tin slot bị conflict
    /// </summary>
    public SlotInfo Slot { get; set; } = new();
    
    /// <summary>
    /// Loại conflict: "MatchConflict", "ParticipantActivityConflict", "LocationConflict"
    /// </summary>
    public string ConflictType { get; set; } = string.Empty;
    
    /// <summary>
    /// Lý do conflict chi tiết
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Match bị conflict (nếu ConflictType = "MatchConflict")
    /// </summary>
    public MatchConflictInfo? MatchConflict { get; set; }
    
    /// <summary>
    /// Activity bị conflict (nếu ConflictType = "ParticipantActivityConflict")
    /// </summary>
    public ActivityConflictInfo? ActivityConflict { get; set; }
    
    /// <summary>
    /// Danh sách participants bị conflict
    /// </summary>
    public List<ParticipantConflictInfo> ParticipantConflicts { get; set; } = new();
}

/// <summary>
/// Thông tin slot
/// </summary>
public class SlotInfo
{
    public DateTime MatchDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Location { get; set; }
}

/// <summary>
/// Thông tin match bị conflict
/// </summary>
public class MatchConflictInfo
{
    public int MatchId { get; set; }
    public int? MatchNumber { get; set; }
    public int? ClassGroup1Id { get; set; }
    public int? ClassGroup2Id { get; set; }
    public DateTime MatchDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Thông tin activity bị conflict
/// </summary>
public class ActivityConflictInfo
{
    public int ActivityId { get; set; }
    public string? ActivityTitle { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Thông tin participant bị conflict
/// </summary>
public class ParticipantConflictInfo
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int ClassGroupId { get; set; }
    public string? ClassGroupName { get; set; }
}

