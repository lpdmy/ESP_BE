using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Models;

namespace EduShpere.Application.DTOs.SearchDto
{
    public class SearchRequestDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Query must be between 1 and 500 characters")]
        public string Query { get; set; } = string.Empty;

        public SearchCategory Category { get; set; } = SearchCategory.All;

        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        
        // New sorting properties
        public SearchSortBy SortByEnum { get; set; } = SearchSortBy.Relevance;

        // Advanced search filters
        public SearchFiltersDto? Filters { get; set; }
    }

    // SearchCategory enum moved to Domain.Models namespace

    public class SearchFiltersDto
    {
        // User filters
        public int? Role { get; set; }
        public int? ClassGroupId { get; set; }
        public int? EnrollmentYear { get; set; }

        // Post filters
        public int? AuthorId { get; set; }
        public int? PostStatus { get; set; }
        public int? PrivacyLevel { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Activity filters
        public int? ActivityCategory { get; set; }
        public int? ClubId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }

        // General filters
        public bool? IsActive { get; set; }
        public bool? IsTrending { get; set; }
    }
}
