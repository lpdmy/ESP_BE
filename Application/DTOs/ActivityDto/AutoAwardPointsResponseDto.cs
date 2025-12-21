namespace EduShpere.Application.DTOs.ActivityDto
{
    /// <summary>
    /// Response DTO cho API tự động cộng điểm từ Power Automate
    /// </summary>
    public class AutoAwardPointsResponseDto
    {
        public bool Success { get; set; }
        public int ActivityId { get; set; }
        public string? ActivityTitle { get; set; }
        public int AwardedCount { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Response DTO cho API batch cộng điểm từ Power Automate
    /// </summary>
    public class BatchAutoAwardPointsResponseDto
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }
        public int TotalAwarded { get; set; }
        public int TotalErrors { get; set; }
        public string? Message { get; set; }
        public List<AutoAwardPointsResponseDto> Results { get; set; } = new();
    }
}

