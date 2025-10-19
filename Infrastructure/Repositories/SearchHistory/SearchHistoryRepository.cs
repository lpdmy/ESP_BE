using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories.SearchHistory
{
    public class SearchHistoryRepository : BaseRepository<Domain.Models.SearchHistory>, ISearchHistoryRepository
    {
        public SearchHistoryRepository(EduShpereDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Domain.Models.SearchHistory>> GetUserSearchHistoryAsync(int userId, int limit = 10)
        {
            return await _dbSet
                .Where(sh => sh.UserId == userId && !sh.IsDeleted)
                .OrderByDescending(sh => sh.SearchedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetRecentSearchesAsync(int userId, string partialQuery, int limit = 10)
        {
            return await _dbSet
                .Where(sh => sh.UserId == userId && 
                           !sh.IsDeleted && 
                           sh.Query.ToLower().Contains(partialQuery.ToLower()))
                .OrderByDescending(sh => sh.SearchedAt)
                .Select(sh => sh.Query)
                .Distinct()
                .Take(limit)
                .ToListAsync();
        }

        public async Task<bool> SaveSearchAsync(string query, int userId, SearchCategory category, int resultCount)
        {
            try
            {
                var searchHistory = new Domain.Models.SearchHistory
                {
                    Query = query,
                    UserId = userId,
                    Category = category,
                    ResultCount = resultCount,
                    SearchedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(searchHistory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Domain.Models.SearchHistory>> GetSearchHistoryByQueryAsync(string query, int limit = 10)
        {
            return await _dbSet
                .Where(sh => !sh.IsDeleted && sh.Query.ToLower().Contains(query.ToLower()))
                .OrderByDescending(sh => sh.SearchedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
