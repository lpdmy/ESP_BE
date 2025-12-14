using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IActivityDraftRepository
    {
        Task<IEnumerable<ActivityDraft>> GetAllByUserIdAsync(int userId);
        Task<ActivityDraft?> GetByIdAsync(int id);
        Task<ActivityDraft?> GetByIdAndUserIdAsync(int id, int userId);
        Task AddAsync(ActivityDraft draft);
        Task UpdateAsync(ActivityDraft draft);
        Task DeleteAsync(int id);
        IQueryable<ActivityDraft> GetQueryable();
    }
}

