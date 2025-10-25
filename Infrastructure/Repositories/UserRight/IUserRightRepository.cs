using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IUserRightRepository
    {
        Task<IEnumerable<UserRight>> GetAllAsync();
        Task<UserRight?> GetByIdAsync(int id);
        Task AddAsync(UserRight entity);
        Task AddRangeAsync(IEnumerable<UserRight> entities);
        Task UpdateAsync(UserRight entity);
        Task DeleteAsync(int id);
    }
}
