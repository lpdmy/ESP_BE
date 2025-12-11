using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(EduShpereDbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<Comment> GetCommentsByPostIdAsync(int postId)
        {
            return _dbSet
                .Include(c => c.User)
                .Where(c => c.PostId == postId && !c.IsDeleted && c.ParentCommentId==null).OrderByDescending(p=>p.CreatedAt);
        }
        public IQueryable<Comment> GetCommentsByCommentIdAsync(int parentId)
        {
            return _dbSet
                .Include(c => c.User)
                .Where(c => c.ParentCommentId == parentId && !c.IsDeleted).OrderByDescending(p => p.CreatedAt);
        }
        public async Task DeleteSoft(int commentId)
        {
            var comment = _dbSet.FirstOrDefault(c => c.Id == commentId);  
            comment.IsDeleted = true;
            await _context.SaveChangesAsync();
        }


    }
}
