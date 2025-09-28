using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories.SearchAnalytics
{
    public class SearchAnalyticsRepository : BaseRepository<Domain.Models.SearchAnalytics>, ISearchAnalyticsRepository
    {
        public SearchAnalyticsRepository(EduShpereDbContext context) : base(context)
        {
        }

        public async Task<Domain.Models.SearchAnalytics?> GetByQueryAsync(string query, SearchCategory category)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sa => sa.Query.ToLower() == query.ToLower() && 
                                         sa.Category == category && 
                                         !sa.IsDeleted);
        }

        public async Task<bool> IncrementSearchCountAsync(string query, SearchCategory category)
        {
            try
            {
                var analytics = await GetByQueryAsync(query, category);
                
                if (analytics == null)
                {
                    analytics = new Domain.Models.SearchAnalytics
                    {
                        Query = query,
                        Category = category,
                        SearchCount = 1,
                        UniqueUsersCount = 1,
                        FirstSearched = DateTime.UtcNow,
                        LastSearched = DateTime.UtcNow
                    };
                    await _dbSet.AddAsync(analytics);
                }
                else
                {
                    analytics.SearchCount++;
                    analytics.LastSearched = DateTime.UtcNow;
                    _dbSet.Update(analytics);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Domain.Models.SearchAnalytics>> GetTrendingSearchesAsync(int limit = 10)
        {
            return await _dbSet
                .Where(sa => !sa.IsDeleted && sa.IsTrending)
                .OrderByDescending(sa => sa.TrendingScore)
                .ThenByDescending(sa => sa.SearchCount)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Models.SearchAnalytics>> GetPopularSearchesAsync(int limit = 10)
        {
            return await _dbSet
                .Where(sa => !sa.IsDeleted)
                .OrderByDescending(sa => sa.SearchCount)
                .ThenByDescending(sa => sa.LastSearched)
                .Take(limit)
                .ToListAsync();
        }

        public async Task UpdateTrendingScoresAsync()
        {
            var analytics = await _dbSet
                .Where(sa => !sa.IsDeleted)
                .ToListAsync();

            foreach (var item in analytics)
            {
                // Calculate trending score based on recent searches and growth
                var hoursSinceLastSearch = (DateTime.UtcNow - item.LastSearched).TotalHours;
                var hoursSinceFirstSearch = (DateTime.UtcNow - item.FirstSearched).TotalHours;
                
                // Trending score: recent searches weighted more heavily
                item.TrendingScore = item.SearchCount / Math.Max(1, hoursSinceFirstSearch) * 
                                   Math.Exp(-hoursSinceLastSearch / 24); // Decay over 24 hours
                
                // Mark as trending if score > threshold
                item.IsTrending = item.TrendingScore > 1.0;
            }

            _dbSet.UpdateRange(analytics);
            await _context.SaveChangesAsync();
        }
    }
}
