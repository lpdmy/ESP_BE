using EduShpere.Domain.Models;

namespace EduShpere.Application.DTOs.SearchDto
{
    public class SearchResponseDto
    {
        public SearchResultsDto Results { get; set; } = new();
        public SearchMetadataDto Metadata { get; set; } = new();
    }

    public class SearchResultsDto
    {
        public IEnumerable<UserSearchResultDto> Users { get; set; } = new List<UserSearchResultDto>();
        public IEnumerable<PostSearchResultDto> Posts { get; set; } = new List<PostSearchResultDto>();
        public IEnumerable<ActivitySearchResultDto> Activities { get; set; } = new List<ActivitySearchResultDto>();
        public IEnumerable<ClubSearchResultDto> Clubs { get; set; } = new List<ClubSearchResultDto>();
        public IEnumerable<HashtagSearchResultDto> Hashtags { get; set; } = new List<HashtagSearchResultDto>();
    }

    public class SearchMetadataDto
    {
        public string Query { get; set; } = string.Empty;
        public SearchCategory Category { get; set; }
        public int TotalResults { get; set; }
        public double SearchTime { get; set; } // in seconds
        public DateTime SearchTimestamp { get; set; } = DateTime.UtcNow;
    }

    // Individual search result DTOs
    public class UserSearchResultDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public string? AvatarUrl { get; set; }
        public string? StudentNumber { get; set; }
        public string? TeacherCode { get; set; }
        public string? ClassName { get; set; }
        public string? Department { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PostSearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public int? Status { get; set; }
        public int? PrivacyLevel { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public IEnumerable<string> Hashtags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public string? Highlight { get; set; } // Search term highlight
    }

    public class ActivitySearchResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public string? Organizer { get; set; }
        public int MaxParticipants { get; set; }
        public int ParticipantsCount { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? Category { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ClubSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public int MembersCount { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class HashtagSearchResultDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PostsCount { get; set; }
        public bool Trending { get; set; }
        public DateTime? LastUsed { get; set; }
        public string? Description { get; set; }
    }
}
