using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IClubCreationRepository
    {
        Task<IEnumerable<ClubCreationRequest>> GetAllAsync();
        Task<ClubCreationRequest?> GetByIdAsync(int id);
        Task AddAsync(ClubCreationRequest entity);
        Task AddRangeAsync(IEnumerable<ClubCreationRequest> entities);
        Task UpdateAsync(ClubCreationRequest entity);
        Task DeleteAsync(int id);
        Task<bool> HasRecentSendCreationRequest(int userid, DateTime oneWeak);
        IQueryable<ClubCreationRequest> GetAllWithIncludes();
    }
}
