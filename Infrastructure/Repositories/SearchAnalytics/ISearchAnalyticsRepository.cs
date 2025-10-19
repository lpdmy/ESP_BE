using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories.SearchAnalytics
{
    public interface ISearchAnalyticsRepository : IRepository<Domain.Models.SearchAnalytics>
    {
        Task<Domain.Models.SearchAnalytics?> GetByQueryAsync(string query, SearchCategory category);
        Task<bool> IncrementSearchCountAsync(string query, SearchCategory category);
        Task<IEnumerable<Domain.Models.SearchAnalytics>> GetTrendingSearchesAsync(int limit = 10);
        Task<IEnumerable<Domain.Models.SearchAnalytics>> GetPopularSearchesAsync(int limit = 10);
        Task UpdateTrendingScoresAsync();
    }
}
