using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IClubCategoryRepository
    {
        Task<IEnumerable<ClubCategory>> GetAllAsync();
        Task<ClubCategory?> GetByIdAsync(int id);
        Task AddAsync(ClubCategory entity);
        Task AddRangeAsync(IEnumerable<ClubCategory> entities);
        Task UpdateAsync(ClubCategory entity);
        Task DeleteAsync(int id);
    }
}
