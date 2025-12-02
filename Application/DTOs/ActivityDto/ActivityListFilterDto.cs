using System.ComponentModel.DataAnnotations;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.DTOs.ActivityDto
{
    /// <summary>
    /// DTO cho filter parameters khi lấy danh sách activities
    /// Kế thừa từ PaginationRequestDto để có validation
    /// </summary>
    public class ActivityListFilterDto : PaginationRequestDto
    {
        public string? SubType { get; set; } // "all", "SeminarWorkshop", "CreativeContest", "SportsFestival"
        public string? Status { get; set; } // "all", "upcoming", "ongoing", "ended"
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Organizer { get; set; }
        public int? MinParticipants { get; set; }
        public int? MaxParticipants { get; set; }
        // SortBy và SortDescending đã được kế thừa từ PaginationRequestDto
        // SortBy mặc định: "StartDate"
        // SortDescending mặc định: true (DESC)
    }
}

