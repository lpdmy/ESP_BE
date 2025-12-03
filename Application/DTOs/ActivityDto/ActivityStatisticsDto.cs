namespace EduShpere.Application.DTOs.ActivityDto
{
    /// <summary>
    /// DTO cho thống kê hoạt động
    /// </summary>
    public class ActivityStatisticsDto
    {
        /// <summary>
        /// Số lượng hoạt động đang diễn ra
        /// </summary>
        public int OngoingCount { get; set; }

        /// <summary>
        /// Số lượng hoạt động sắp tới
        /// </summary>
        public int UpcomingCount { get; set; }

        /// <summary>
        /// Số lượng hoạt động đã hoàn thành
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// Tổng số người tham gia tất cả hoạt động
        /// </summary>
        public int TotalParticipants { get; set; }
    }
}

