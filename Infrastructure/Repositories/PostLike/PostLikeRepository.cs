

using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class PostLikeRepository : BaseRepository<PostLike>, IPostLikeRepository
    {
        public PostLikeRepository(EduShpereDbContext context) : base(context)
        {
        }

        public async Task<PostLike> CheckExist(int postId , int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);
        }
        
    }
}
