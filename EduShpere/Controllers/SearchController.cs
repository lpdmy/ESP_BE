using EduShpere.Application;
using EduShpere.Application.DTOs.SearchDto;
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Application.Services.SearchService;
using EduShpere.Infrastructure;
using EduShpere.Middlewares;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;

namespace EduShpere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [CustomModelValidationFilter]
    public class SearchController : BaseController
    {
        private readonly ISearchService _searchService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public SearchController(ISearchService searchService, IUserRepository userRepository, IMapper mapper)
        {
            _searchService = searchService;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Global search across all entities
        /// </summary>
        /// <param name="request">Search parameters</param>
        /// <returns>Search results</returns>
        [HttpGet]
        [AllowAnonymous] // Allow without authentication for testing
        public async Task<IActionResult> GlobalSearch([FromQuery] SearchRequestDto request)
        {
            try
            {
                Console.WriteLine($"GlobalSearch called with query: '{request.Query}', category: {request.Category}, sortBy: {request.SortByEnum}");
                
                // Add validation logging
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new ResponseDto<string>(null, "Query không được để trống", 400));
                }

                if (request.Query.Length < 1)
                {
                    return BadRequest(new ResponseDto<string>(null, "Query phải có ít nhất 1 ký tự", 400));
                }

                var result = await _searchService.GlobalSearchAsync(request);

                // Save search analytics
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    _ = Task.Run(() => _searchService.SaveSearchAnalyticsAsync(
                        request.Query, 
                        request.Category, 
                        userId, 
                        result.Metadata.TotalResults
                    ));
                }

                Console.WriteLine($"Returning search results: Users={result.Results.Users.Count()}, Posts={result.Results.Posts.Count()}");
                return Ok(new ResponseDto<SearchResponseDto>(result, "Tìm kiếm thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Get search suggestions for autocomplete
        /// </summary>
        /// <param name="request">Search suggestions request</param>
        /// <returns>Search suggestions</returns>
        [HttpGet("suggestions")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] SearchSuggestionsRequestDto request)
        {
            try
            {
                var suggestions = await _searchService.GetSearchSuggestionsAsync(request.Query, request.Limit);
                return Ok(new ResponseDto<SearchSuggestionsDto>(suggestions, "Gợi ý tìm kiếm thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi lấy gợi ý: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Get trending searches
        /// </summary>
        /// <param name="limit">Number of trending items to return</param>
        /// <returns>Trending searches</returns>
        [HttpGet("trending")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrendingSearches([FromQuery] int limit = 10)
        {
            try
            {
                var trending = await _searchService.GetTrendingSearchesAsync(limit);
                return Ok(new ResponseDto<TrendingSearchesDto>(trending, "Lấy trending searches thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi lấy trending: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Search posts with advanced filters
        /// </summary>
        /// <param name="request">Post search parameters with filters</param>
        /// <returns>Post search results with pagination</returns>
        [HttpPost("posts")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchPosts([FromBody] PostSearchDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new ResponseDto<string>(null, "Query không được để trống", 400));
                }

                if (request.Query.Length < 1)
                {
                    return BadRequest(new ResponseDto<string>(null, "Query phải có ít nhất 1 ký tự", 400));
                }

                var result = await _searchService.SearchPostsWithFiltersAsync(request);

                // Save search analytics
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    _ = Task.Run(() => _searchService.SaveSearchAnalyticsAsync(
                        request.Query, 
                        Domain.Models.SearchCategory.Posts, 
                        userId, 
                        result.TotalCount
                    ));
                }

                return Ok(new ResponseDto<PostAdvancedSearchResponseDto>(result, "Tìm kiếm bài đăng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm bài đăng: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Search users only
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="pageSize">Number of results</param>
        /// <returns>User search results</returns>
        [HttpGet("users")]
        [AllowAnonymous] // Allow without authentication for testing
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest(new ResponseDto<string>(null, "Query phải có ít nhất 2 ký tự", 400));
                }

                var result = await _searchService.SearchUsersAsync(query, pageSize);
                return Ok(new ResponseDto<IEnumerable<UserSearchResultDto>>(result, "Tìm kiếm người dùng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm người dùng: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Search posts only
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="pageSize">Number of results</param>
        /// <returns>Post search results</returns>
        [HttpGet("posts")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> SearchPosts([FromQuery] string query, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest(new ResponseDto<string>(null, "Query phải có ít nhất 2 ký tự", 400));
                }

                var result = await _searchService.SearchPostsAsync(query, pageSize);
                return Ok(new ResponseDto<IEnumerable<PostSearchResultDto>>(result, "Tìm kiếm bài viết thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm bài viết: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Search activities only
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="pageSize">Number of results</param>
        /// <returns>Activity search results</returns>
        [HttpGet("activities")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> SearchActivities([FromQuery] string query, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest(new ResponseDto<string>(null, "Query phải có ít nhất 2 ký tự", 400));
                }

                var result = await _searchService.SearchActivitiesAsync(query, pageSize);
                return Ok(new ResponseDto<IEnumerable<ActivitySearchResultDto>>(result, "Tìm kiếm hoạt động thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm hoạt động: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Search clubs only
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="pageSize">Number of results</param>
        /// <returns>Club search results</returns>
        [HttpGet("clubs")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> SearchClubs([FromQuery] string query, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest(new ResponseDto<string>(null, "Query phải có ít nhất 2 ký tự", 400));
                }

                var result = await _searchService.SearchClubsAsync(query, pageSize);
                return Ok(new ResponseDto<IEnumerable<ClubSearchResultDto>>(result, "Tìm kiếm câu lạc bộ thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm câu lạc bộ: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Search hashtags only
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="pageSize">Number of results</param>
        /// <returns>Hashtag search results</returns>
        [HttpGet("hashtags")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> SearchHashtags([FromQuery] string query, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest(new ResponseDto<string>(null, "Query phải có ít nhất 2 ký tự", 400));
                }

                var result = await _searchService.SearchHashtagsAsync(query, pageSize);
                return Ok(new ResponseDto<IEnumerable<HashtagSearchResultDto>>(result, "Tìm kiếm hashtag thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm hashtag: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Get search suggestions
        /// </summary>
        /// <param name="query">Partial search query</param>
        /// <returns>Search suggestions</returns>
        [HttpGet("suggestions-old")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetSearchSuggestionsOld([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Ok(new ResponseDto<IEnumerable<string>>(new List<string>(), "Không có gợi ý"));
                }

                var result = await _searchService.GetSearchSuggestionsAsync(query, 10);
                var suggestions = new List<string>();
                suggestions.AddRange(result.Users);
                suggestions.AddRange(result.Posts);
                suggestions.AddRange(result.Activities);
                suggestions.AddRange(result.Hashtags);
                suggestions.AddRange(result.RecentSearches);
                return Ok(new ResponseDto<IEnumerable<string>>(suggestions, "Lấy gợi ý tìm kiếm thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi lấy gợi ý tìm kiếm: {ex.Message}", 400));
            }
        }

        /// <summary>
        /// Test endpoint to debug search functionality
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestSearch([FromQuery] string query = "nguyen")
        {
            try
            {
                Console.WriteLine($"TestSearch called with query: '{query}'");
                
                // Test direct repository call
                var users = await _userRepository.SearchAsync(query, 10);
                Console.WriteLine($"Direct repository search found {users.Count()} users");
                
                // Test manual mapping instead of AutoMapper
                var userDtos = users.Select(u => new UserSearchResultDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    Email = u.Email ?? "",
                    Role = u.Role.HasValue ? (int)u.Role.Value : 0,
                    AvatarUrl = u.AvatarUrl,
                    CreatedAt = u.CreatedAt ?? DateTime.MinValue,
                    StudentNumber = u.StudentProfile?.StudentNumber,
                    TeacherCode = u.TeacherProfile?.TeacherCode,
                    ClassName = u.ClassGroupMembers?.FirstOrDefault()?.ClassGroup?.Name,
                    Department = u.TeacherProfile?.Department
                }).ToList();
                
                Console.WriteLine($"Manual mapping resulted in {userDtos.Count} DTOs");
                
                return Ok(new { 
                    message = "Test search completed",
                    query = query,
                    rawUsers = users.Count(),
                    mappedUsers = userDtos.Count(),
                    users = userDtos
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestSearch error: {ex.Message}");
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Test endpoint to debug search functionality with specific query
        /// </summary>
        [HttpGet("test-le-my")]
        [AllowAnonymous]
        public async Task<IActionResult> TestLeMySearch()
        {
            try
            {
                Console.WriteLine("TestLeMySearch called");
                
                // Test with "Le My" query
                var query = "Le My";
                var users = await _userRepository.SearchAsync(query, 10);
                Console.WriteLine($"Search for '{query}' found {users.Count()} users");
                
                // Test with "Lê My" query (with accent)
                var queryWithAccent = "Lê My";
                var usersWithAccent = await _userRepository.SearchAsync(queryWithAccent, 10);
                Console.WriteLine($"Search for '{queryWithAccent}' found {usersWithAccent.Count()} users");
                
                // Test with individual words
                var queryLe = "Le";
                var usersLe = await _userRepository.SearchAsync(queryLe, 10);
                Console.WriteLine($"Search for '{queryLe}' found {usersLe.Count()} users");
                
                var queryMy = "My";
                var usersMy = await _userRepository.SearchAsync(queryMy, 10);
                Console.WriteLine($"Search for '{queryMy}' found {usersMy.Count()} users");
                
                return Ok(new { 
                    message = "Test Le My search completed",
                    results = new {
                        leMy = users.Count(),
                        leMyWithAccent = usersWithAccent.Count(),
                        le = usersLe.Count(),
                        my = usersMy.Count(),
                        leMyUsers = users.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email }),
                        leUsers = usersLe.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email }),
                        myUsers = usersMy.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email })
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestLeMySearch error: {ex.Message}");
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Debug endpoint to check all users in database
        /// </summary>
        [HttpGet("debug-all-users")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugAllUsers()
        {
            try
            {
                Console.WriteLine("DebugAllUsers called");
                
                // Get all users from repository
                var allUsers = await _userRepository.GetAllAsync();
                Console.WriteLine($"Total users in DB: {allUsers.Count()}");
                
                // Filter users with "Le" or "My" in name
                var leMyUsers = allUsers.Where(u => 
                    (u.FirstName != null && (u.FirstName.ToLower().Contains("le") || u.FirstName.ToLower().Contains("lê") || u.FirstName.ToLower().Contains("my"))) ||
                    (u.LastName != null && (u.LastName.ToLower().Contains("le") || u.LastName.ToLower().Contains("lê") || u.LastName.ToLower().Contains("my")))
                ).ToList();
                
                Console.WriteLine($"Users with Le/My in name: {leMyUsers.Count()}");
                
                return Ok(new { 
                    message = "Debug all users completed",
                    totalUsers = allUsers.Count(),
                    leMyUsersCount = leMyUsers.Count(),
                    leMyUsers = leMyUsers.Select(u => new { 
                        u.Id, 
                        u.FirstName, 
                        u.LastName, 
                        u.Email,
                        firstNameTrimmed = u.FirstName?.Trim(),
                        lastNameTrimmed = u.LastName?.Trim(),
                        fullName = $"{u.FirstName?.Trim()} {u.LastName?.Trim()}".Trim()
                    }),
                    allUsers = allUsers.Take(10).Select(u => new { 
                        u.Id, 
                        u.FirstName, 
                        u.LastName, 
                        u.Email 
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DebugAllUsers error: {ex.Message}");
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Advanced search with filters
        /// </summary>
        /// <param name="request">Advanced search parameters</param>
        /// <returns>Filtered search results</returns>
        [HttpPost("advanced")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> AdvancedSearch([FromBody] SearchRequestDto request)
        {
            try
            {
                var result = await _searchService.GlobalSearchAsync(request);

                // Save search analytics
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    _ = Task.Run(() => _searchService.SaveSearchAnalyticsAsync(
                        request.Query, 
                        request.Category, 
                        userId, 
                        result.Metadata.TotalResults
                    ));
                }

                return Ok(new ResponseDto<SearchResponseDto>(result, "Tìm kiếm nâng cao thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"Lỗi tìm kiếm nâng cao: {ex.Message}", 400));
            }
        }
        
    }
}
