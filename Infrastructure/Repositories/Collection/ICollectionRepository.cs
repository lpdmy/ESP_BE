using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface ICollectionRepository
    {
        Task<IEnumerable<FavoriteCollection>> GetAllAsync();
        Task<FavoriteCollection?> GetByIdAsync(int id);
        Task AddAsync(FavoriteCollection entity);
        Task AddRangeAsync(IEnumerable<FavoriteCollection> entities);
        Task UpdateAsync(FavoriteCollection entity);
        Task DeleteAsync(int id);
        IQueryable<FavoriteCollection> GetByUserIdQuery(User user);
        Task DeleteSoft(int id);
    }
}
