using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories.SearchHistory
{
    public interface ISearchHistoryRepository : IRepository<Domain.Models.SearchHistory>
    {
        Task<IEnumerable<Domain.Models.SearchHistory>> GetUserSearchHistoryAsync(int userId, int limit = 10);
        Task<IEnumerable<string>> GetRecentSearchesAsync(int userId, string partialQuery, int limit = 10);
        Task<bool> SaveSearchAsync(string query, int userId, SearchCategory category, int resultCount);
        Task<IEnumerable<Domain.Models.SearchHistory>> GetSearchHistoryByQueryAsync(string query, int limit = 10);
    }
}
