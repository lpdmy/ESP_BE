using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.PostDto
{
    public class PostSearchDto
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Query { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public PostSearchSortBy SortBy { get; set; } = PostSearchSortBy.Relevance;
        public bool SortDescending { get; set; } = true;

        public PostSearchFiltersDto? Filters { get; set; }
    }

    public class PostSearchFiltersDto
    {
        public int? AuthorId { get; set; }
        public int? ClassGroupId { get; set; }
        public int? ClubId { get; set; }
        public PostStatus? Status { get; set; }
        public PostVisibility? PrivacyLevel { get; set; }
        public List<string>? Hashtags { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? HasAttachments { get; set; }
        public bool? HasComments { get; set; }
        public int? MinLikeCount { get; set; }
        public int? MinCommentsCount { get; set; }
    }

    public enum PostSearchSortBy
    {
        Relevance = 0,
        MostRecent = 1,
        MostPopular = 2,
        MostCommented = 3,
        MostLiked = 4,
        Alphabetical = 5
    }

    public class PostAdvancedSearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public PostStatus Status { get; set; }
        public PostVisibility PrivacyLevel { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public List<string> Hashtags { get; set; } = new();
        public List<string> AttachmentUrls { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string? Highlight { get; set; } // Search term highlight
        public double RelevanceScore { get; set; }
    }

    public class PostAdvancedSearchResponseDto
    {
        public List<PostAdvancedSearchResultDto> Posts { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string Query { get; set; } = string.Empty;
        public double SearchTime { get; set; } // in seconds
        public DateTime SearchTimestamp { get; set; } = DateTime.UtcNow;
    }
}
