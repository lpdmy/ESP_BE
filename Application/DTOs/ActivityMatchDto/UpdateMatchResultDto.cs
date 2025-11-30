using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityMatchDto
{
    public class UpdateMatchResultDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Điểm phải >= 0")]
        public int Score1 { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Điểm phải >= 0")]
        public int Score2 { get; set; }

        /// <summary>
        /// Khi true (mặc định), hệ thống yêu cầu WinnerClassGroupId và sẽ kết thúc trận đấu.
        /// Khi false, chỉ cập nhật tỉ số và giữ trạng thái InProgress.
        /// </summary>
        public bool MarkAsCompleted { get; set; } = true;

        public int? WinnerClassGroupId { get; set; }
    }
}

