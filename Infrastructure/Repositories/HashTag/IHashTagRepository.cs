using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IHashTagRepository
    {
        Task<IEnumerable<Hashtag>> GetAllAsync();
        Task<Hashtag?> GetByIdAsync(int id);
        Task AddAsync(Hashtag entity);
        Task AddRangeAsync(IEnumerable<Hashtag> entities);
        Task UpdateAsync(Hashtag entity);
        Task DeleteAsync(int id);
        Task<Hashtag?> GetByNameAsync(string name);
    }
}
