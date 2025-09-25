
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Post>> getAllPostIncluding()
        {
            return await _dbSet
                .Include(p => p.Club)
                .Include(p => p.User)
                .Include(p => p.Attachments)
                .Include(p => p.PostLikes)
                .Include(p => p.PostReports)
                .Include(p => p.Comments)
                .Include(p => p.PostHashtags)
                    .ThenInclude(ph => ph.Hashtag)
                .Include(p => p.PostMentions)
                    .ThenInclude(pm => pm.MentionedUser)
                .ToListAsync();
        }
        public async Task DeleteSoft(int id)
        {
            var posts = _dbSet.Where(p => p.Id == id).FirstOrDefault();
            posts.IsDeleted = true;
            await _context.SaveChangesAsync();
        }

    }
}
