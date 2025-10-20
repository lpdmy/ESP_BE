using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task AddAsync(Comment entity);
        Task AddRangeAsync(IEnumerable<Comment> entities);
        Task UpdateAsync(Comment entity);
        Task DeleteAsync(int id);
        IQueryable<Comment> GetCommentsByPostIdAsync(int postId);
        IQueryable<Comment> GetCommentsByCommentIdAsync(int parentId);
        Task DeleteSoft(int commentId);
    }
}
