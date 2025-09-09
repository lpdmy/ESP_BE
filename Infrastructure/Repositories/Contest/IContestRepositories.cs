
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IContestRepositories
    {
        Task<IEnumerable<Activity>> GetAllAsync();
        Task<Activity?> GetByIdAsync(int id);
        Task AddAsync(Activity contest);
        Task UpdateAsync(Activity contest);
        Task DeleteAsync(int id);
    }
}
