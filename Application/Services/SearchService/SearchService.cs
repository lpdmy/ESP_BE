using AutoMapper;
using EduShpere.Application.DTOs.SearchDto;
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Application.Services.RankingService;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories.SearchAnalytics;
using EduShpere.Infrastructure.Repositories.SearchHistory;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EduShpere.Application.Services.SearchService
{
    public class SearchService : ISearchService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly EduShpere.Infrastructure.Repositories.IActivityRepository _activityRepository;
        private readonly ISearchHistoryRepository _searchHistoryRepository;
        private readonly ISearchAnalyticsRepository _searchAnalyticsRepository;
        private readonly IRankingService _rankingService;
        private readonly IHttpContextService _httpContextService;
        private readonly IMapper _mapper;

        public SearchService(
            IUserRepository userRepository,
            IPostRepository postRepository,
            EduShpere.Infrastructure.Repositories.IActivityRepository activityRepository,
            ISearchHistoryRepository searchHistoryRepository,
            ISearchAnalyticsRepository searchAnalyticsRepository,
            IRankingService rankingService,
            IHttpContextService httpContextService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _activityRepository = activityRepository;
            _searchHistoryRepository = searchHistoryRepository;
            _searchAnalyticsRepository = searchAnalyticsRepository;
            _rankingService = rankingService;
            _httpContextService = httpContextService;
            _mapper = mapper;
        }

        public async Task<SearchResponseDto> GlobalSearchAsync(SearchRequestDto request)
        {
            var stopwatch = Stopwatch.StartNew();
            var results = new SearchResultsDto();

            try
            {
                // Calculate per-category limit for global search
                var perCategoryLimit = request.Category == Domain.Models.SearchCategory.All 
                    ? Math.Max(5, request.PageSize / 2)  // Give more results per category, split into 2 main groups instead of 5
                    : request.PageSize;

                // Execute searches sequentially to avoid DbContext threading issues
                if (request.Category == Domain.Models.SearchCategory.All || request.Category == Domain.Models.SearchCategory.Users)
                {
                    try 
                    {
                        results.Users = await SearchUsersAsync(request.Query, perCategoryLimit);
                    }
                    catch (Exception ex)
                    {
                        // Skip user search if it fails, continue with other searches
                        Console.WriteLine($"User search failed: {ex.Message}");
                        results.Users = new List<UserSearchResultDto>();
                    }
                }

                if (request.Category == Domain.Models.SearchCategory.All || request.Category == Domain.Models.SearchCategory.Posts)
                {
                    results.Posts = await SearchPostsAsync(request.Query, perCategoryLimit);
                }

                if (request.Category == Domain.Models.SearchCategory.All || request.Category == Domain.Models.SearchCategory.Activities)
                {
                    results.Activities = await SearchActivitiesAsync(request.Query, perCategoryLimit);
                }

                if (request.Category == Domain.Models.SearchCategory.All || request.Category == Domain.Models.SearchCategory.Clubs)
                {
                    results.Clubs = await SearchClubsAsync(request.Query, perCategoryLimit);
                }

                if (request.Category == Domain.Models.SearchCategory.All || request.Category == Domain.Models.SearchCategory.Hashtags)
                {
                    results.Hashtags = await SearchHashtagsAsync(request.Query, perCategoryLimit);
                }

                stopwatch.Stop();

                // Calculate total results
                var totalResults = results.Users.Count() + results.Posts.Count() + 
                                 results.Activities.Count() + results.Clubs.Count() + 
                                 results.Hashtags.Count();

                return new SearchResponseDto
                {
                    Results = results,
                    Metadata = new SearchMetadataDto
                    {
                        Query = request.Query,
                        Category = request.Category,
                        TotalResults = totalResults,
                        SearchTime = stopwatch.Elapsed.TotalSeconds
                    }
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                throw new Exception($"Search failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<UserSearchResultDto>> SearchUsersAsync(string query, int pageSize = 10)
        {
            try
            {
                Console.WriteLine($"Searching users with query: '{query}', pageSize: {pageSize}");
                var users = await _userRepository.SearchAsync(query, pageSize * 2); // Get more for ranking
                Console.WriteLine($"Found {users.Count()} users from repository");
                
                // Log some sample users for debugging
                if (users.Any())
                {
                    var sampleUsers = users.Take(3).Select(u => 
                        $"ID:{u.Id}, FirstName:'{u.FirstName}', LastName:'{u.LastName}', Email:'{u.Email}'"
                    );
                    Console.WriteLine($"Sample users from repository: {string.Join("; ", sampleUsers)}");
                }
                
                // Get current user ID for ranking
                var userIdClaim = _httpContextService.GetCurrentUserId();
                var currentUserId = userIdClaim ?? 0;
                
                // Rank users
                var rankedUsers = await _rankingService.RankUsersAsync(users, query, currentUserId);
                Console.WriteLine($"After ranking: {rankedUsers.Count()} users");
                
                // Log some sample ranked users for debugging
                if (rankedUsers.Any())
                {
                    var sampleRankedUsers = rankedUsers.Take(3).Select(u => 
                        $"ID:{u.Id}, FirstName:'{u.FirstName}', LastName:'{u.LastName}', Email:'{u.Email}'"
                    );
                    Console.WriteLine($"Sample ranked users: {string.Join("; ", sampleRankedUsers)}");
                }
                
                var finalResults = rankedUsers.Take(pageSize);
                Console.WriteLine($"Final results: {finalResults.Count()} users");
                
                return finalResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"User search failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new Exception($"User search failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PostSearchResultDto>> SearchPostsAsync(string query, int pageSize = 10)
        {
            try
            {
                var posts = await _postRepository.SearchAsync(query, pageSize * 2); // Get more for ranking
                
                // Get current user ID for ranking
                var userIdClaim = _httpContextService.GetCurrentUserId();
                var currentUserId = userIdClaim ?? 0;
                
                // Rank posts
                var rankedPosts = await _rankingService.RankPostsAsync(posts, query, currentUserId);
                
                return rankedPosts.Take(pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Post search failed: {ex.Message}", ex);
            }
        }

        public async Task<PostAdvancedSearchResponseDto> SearchPostsWithFiltersAsync(PostSearchDto request)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Build search query with filters
                var posts = await _postRepository.SearchAsync(request.Query, request.PageSize * 2);
                
                // Get current user ID for ranking
                var userIdClaim = _httpContextService.GetCurrentUserId();
                var currentUserId = userIdClaim ?? 0;
                
                // Rank posts
                var rankedPosts = await _rankingService.RankPostsAsync(posts, request.Query, currentUserId);
                
                // Convert to PostAdvancedSearchResultDto
                var convertedPosts = rankedPosts.Select(p => new PostAdvancedSearchResultDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    AuthorName = p.AuthorName,
                    AuthorId = p.AuthorId,
                    Status = (PostStatus)(p.Status ?? 0),
                    PrivacyLevel = (PostVisibility)(p.PrivacyLevel ?? 0),
                    LikesCount = p.LikesCount,
                    CommentsCount = p.CommentsCount,
                    Hashtags = p.Hashtags.ToList(),
                    AttachmentUrls = new List<string>(), // Will be populated from attachments
                    CreatedAt = p.CreatedAt,
                    Highlight = p.Highlight,
                    RelevanceScore = 0.0 // Will be calculated by ranking service
                }).ToList();
                
                // Apply pagination
                var totalCount = convertedPosts.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
                var paginatedPosts = convertedPosts
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                stopwatch.Stop();

                return new PostAdvancedSearchResponseDto
                {
                    Posts = paginatedPosts,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    Query = request.Query,
                    SearchTime = stopwatch.Elapsed.TotalSeconds,
                    SearchTimestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Post search with filters failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ActivitySearchResultDto>> SearchActivitiesAsync(string query, int pageSize = 10)
        {
            try
            {
                var activities = await _activityRepository.SearchAsync(query, pageSize * 2); // Get more for ranking
                
                // Get current user ID for ranking
                var userIdClaim = _httpContextService.GetCurrentUserId();
                var currentUserId = userIdClaim ?? 0;
                
                // Rank activities
                var rankedActivities = await _rankingService.RankActivitiesAsync(activities, query, currentUserId);
                
                return rankedActivities.Take(pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception($"Activity search failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ClubSearchResultDto>> SearchClubsAsync(string query, int pageSize = 10)
        {
            try
            {
                // This would need a ClubRepository - placeholder implementation
                await Task.Delay(1); // Remove when real implementation is added
                return new List<ClubSearchResultDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Club search failed: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<HashtagSearchResultDto>> SearchHashtagsAsync(string query, int pageSize = 10)
        {
            try
            {
                // This would need a HashtagRepository - placeholder implementation
                await Task.Delay(1); // Remove when real implementation is added
                return new List<HashtagSearchResultDto>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Hashtag search failed: {ex.Message}", ex);
            }
        }

        public async Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string partialQuery, int limit = 10)
        {
            try
            {
                var suggestions = new SearchSuggestionsDto();
                
                // Get user name suggestions
                var users = await _userRepository.SearchAsync(partialQuery, limit);
                suggestions.Users = users.Select(u => $"{u.FirstName} {u.LastName}".Trim())
                                .Where(name => !string.IsNullOrEmpty(name))
                                .Take(limit);

                // Get activity title suggestions
                var activities = await _activityRepository.SearchAsync(partialQuery, limit);
                suggestions.Activities = activities.Select(a => a.Title)
                                          .Where(t => !string.IsNullOrEmpty(t))
                                          .Take(limit);

                // Get post title suggestions
                var posts = await _postRepository.SearchAsync(partialQuery, limit);
                suggestions.Posts = posts.Select(p => p.Title)
                                .Where(t => !string.IsNullOrEmpty(t))
                                .Take(limit);

                // Get recent searches for this user
                var userIdClaim = _httpContextService.GetCurrentUserId();
                if (userIdClaim.HasValue)
                {
                    suggestions.RecentSearches = await _searchHistoryRepository
                        .GetRecentSearchesAsync(userIdClaim.Value, partialQuery, limit);
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                throw new Exception($"Getting search suggestions failed: {ex.Message}", ex);
            }
        }


        public async Task<TrendingSearchesDto> GetTrendingSearchesAsync(int limit = 10)
        {
            try
            {
                var trendingItems = await _searchAnalyticsRepository.GetTrendingSearchesAsync(limit);
                
                return new TrendingSearchesDto
                {
                    Items = trendingItems.Select(item => new TrendingSearchItemDto
                    {
                        Query = item.Query,
                        SearchCount = item.SearchCount,
                        IsTrending = item.IsTrending,
                        LastSearched = item.LastSearched
                    })
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Getting trending searches failed: {ex.Message}", ex);
            }
        }

        // Keep old method for backward compatibility
        public async Task<IEnumerable<string>> GetTrendingSearchesAsync()
        {
            try
            {
                // Placeholder - would need search analytics table
                await Task.Delay(1);
                return new List<string> 
                { 
                    "lập trình", 
                    "cuộc thi", 
                    "học tập", 
                    "thể thao", 
                    "âm nhạc" 
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Getting trending searches failed: {ex.Message}", ex);
            }
        }


        public async Task SaveSearchAnalyticsAsync(string query, Domain.Models.SearchCategory category, int userId, int resultCount)
        {
            try
            {
                // Save to search history
                await _searchHistoryRepository.SaveSearchAsync(query, userId, category, resultCount);
                
                // Update search analytics
                await _searchAnalyticsRepository.IncrementSearchCountAsync(query, category);
            }
            catch (Exception ex)
            {
                // Don't throw - analytics should not break search
                Console.WriteLine($"Failed to save search analytics: {ex.Message}");
            }
        }

        private string GenerateHighlight(string? content, string query, int maxLength = 150)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(query))
                return content?.Substring(0, Math.Min(maxLength, content.Length)) ?? "";

            var queryLower = query.ToLower();
            var contentLower = content.ToLower();
            var index = contentLower.IndexOf(queryLower);

            if (index == -1)
                return content.Substring(0, Math.Min(maxLength, content.Length));

            // Find optimal snippet around the match
            var start = Math.Max(0, index - maxLength / 3);
            var end = Math.Min(content.Length, start + maxLength);
            
            var snippet = content.Substring(start, end - start);
            
            // Add ellipsis if needed
            if (start > 0) snippet = "..." + snippet;
            if (end < content.Length) snippet = snippet + "...";

            return snippet;
        }
    }
}
