using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure
{
    public interface IPostRepository 
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task AddAsync(Post entity);
        Task AddRangeAsync(IEnumerable<Post> entities);
        Task UpdateAsync(Post entity);
        Task DeleteAsync(int id);
        IQueryable<Post> GetAllPostIncluding();
        Task DeleteSoft(int id);
    }
}
