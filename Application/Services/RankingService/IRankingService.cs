using EduShpere.Domain.Models;
using EduShpere.Application.DTOs.SearchDto;

namespace EduShpere.Application.Services.RankingService
{
    public interface IRankingService
    {
        Task<IEnumerable<UserSearchResultDto>> RankUsersAsync(IEnumerable<User> users, string query, int userId);
        Task<IEnumerable<PostSearchResultDto>> RankPostsAsync(IEnumerable<Post> posts, string query, int userId);
        Task<IEnumerable<ActivitySearchResultDto>> RankActivitiesAsync(IEnumerable<Activity> activities, string query, int userId);
        double CalculateRelevanceScore<T>(T item, string query, int userId) where T : class;
    }
}
