using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IActivityRepository 
    {
        Task<IEnumerable<Activity>> GetAllAsync();
        Task<Activity?> GetByIdAsync(int id);
        Task AddAsync(Activity Activity);
        Task UpdateAsync(Activity Activity);
        Task DeleteAsync(int id);
        Task AddRangeAsync(IEnumerable<Activity> Activity);
        Task<Activity?> GetByIdWithIncludesAsync(int id);
        Task<string?> GetGradingSettingsAsync(int id); // Optimized: Get only GradingSettings without loading all data
        Task<IEnumerable<Activity>> SearchAsync(string query, int limit = 10);
        Task<(IEnumerable<Activity> Items, int TotalCount)> GetAllWithPagingAsync(int pageNumber, int pageSize, string? search = null);
        Task<(IEnumerable<Activity> Items, int TotalCount)> GetActivitiesByUserIdAsync(int userId, int pageNumber, int pageSize, string? search = null, string? status = null);
        IQueryable<Activity> GetQueryable();
    }
}
