namespace ESP.AIService.Models;

/// <summary>
/// Model cho training data - TẤT CẢ fields phải là float!
/// </summary>
public class TournamentScheduleData
{
    // Activity features
    public float ActivityId { get; set; }
    public float SportId { get; set; }
    public float NumberOfTeams { get; set; }
    public float NumberOfRounds { get; set; }
    
    // Time features
    public float HourOfDay { get; set; } // 0-23
    public float DayOfWeek { get; set; } // 0-6 (Sunday = 0)
    public float Month { get; set; } // 1-12
    public float DayOfMonth { get; set; } // 1-31
    
    // Match features
    public float Round { get; set; }
    public float MatchNumber { get; set; }
    public float Grade { get; set; } // Grade level (if applicable)
    
    // Location features
    public float LocationHash { get; set; } // Hash của Location string
    
    // Duration features
    public float MatchDurationMinutes { get; set; } // Thời gian thi đấu (phút)
    
    // Success score (Label) - tỷ lệ thành công của match
    // Score cao = Match diễn ra đúng giờ, không conflict, nhiều người tham gia
    public float SuccessScore { get; set; } // 0-1
}

