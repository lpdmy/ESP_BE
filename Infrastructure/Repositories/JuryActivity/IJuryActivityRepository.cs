using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IJuryActivityRepository
    {
        Task<IEnumerable<JuryActivity>> GetAllAsync();
        Task<JuryActivity?> GetByIdAsync(int id);
        Task AddAsync(JuryActivity entity);
        Task AddRangeAsync(IEnumerable<JuryActivity> entities);
        Task UpdateAsync(JuryActivity entity);
        Task DeleteAsync(int id);
        IQueryable<JuryActivity> GetAllJuryActivityByActivityIdIncluding(int activityId);
        IQueryable<JuryActivity> GetAllJuryActivityIncluding();
        Task<bool> IsExisting(int userId, int activityId);
        IQueryable<JuryActivity> GetAllJuryActivityByUser(int id);
        IQueryable<JuryActivity> GetQueryable();

    }
}
