using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IClubRepository
    {
        Task<IEnumerable<Club>> GetAllAsync();
        Task<Club?> GetByIdAsync(int id);
        Task AddAsync(Club entity);
        Task AddRangeAsync(IEnumerable<Club> entities);
        Task UpdateAsync(Club entity);
        Task DeleteAsync(int id);
        IQueryable<Club> GetAllWithIncludes();
        Task<Club?> GetByIdWithIncludesAsync(int id);
        Task<bool> DeleteSoft(int id);
    }
}
