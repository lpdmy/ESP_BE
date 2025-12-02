using EduShpere.Application.DTOs.SearchDto;
using EduShpere.Application.DTOs.PostDto;

namespace EduShpere.Application.Services.SearchService
{
    public interface ISearchService
    {
        /// <summary>
        /// Global search across all entities
        /// </summary>
        Task<SearchResponseDto> GlobalSearchAsync(SearchRequestDto request);

        /// <summary>
        /// Search users only
        /// </summary>
        Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query, int pageSize = 10);

        /// <summary>
        /// Search user by exact email
        /// </summary>
        Task<UserSearchResultDto?> SearchUserByEmailAsync(string email);

        /// <summary>
        /// Search posts only
        /// </summary>
        Task<IEnumerable<PostSearchResultDto>> SearchPostsAsync(string query, int pageSize = 10);

        /// <summary>
        /// Search posts with advanced filters
        /// </summary>
        Task<PostAdvancedSearchResponseDto> SearchPostsWithFiltersAsync(PostSearchDto request);

        /// <summary>
        /// Search activities only
        /// </summary>
        Task<IEnumerable<ActivitySearchResultDto>> SearchActivitiesAsync(string query, int pageSize = 10);

        /// <summary>
        /// Search clubs only
        /// </summary>
        Task<IEnumerable<ClubSearchResultDto>> SearchClubsAsync(string query, int pageSize = 10);

        /// <summary>
        /// Search hashtags only
        /// </summary>
        Task<IEnumerable<HashtagSearchResultDto>> SearchHashtagsAsync(string query, int pageSize = 10);

        /// <summary>
        /// Get search suggestions based on partial query
        /// </summary>
        Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string partialQuery, int limit = 10);

        /// <summary>
        /// Get trending search terms
        /// </summary>
        Task<TrendingSearchesDto> GetTrendingSearchesAsync(int limit = 10);

        /// <summary>
        /// Save search query for analytics
        /// </summary>
        Task SaveSearchAnalyticsAsync(string query, Domain.Models.SearchCategory category, int userId, int resultCount);
    }
}
