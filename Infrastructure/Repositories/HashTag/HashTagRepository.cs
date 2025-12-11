
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class HashTagRepository : BaseRepository<Hashtag>, IHashTagRepository
    {
        public HashTagRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<Hashtag?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(h => h.Name == name);
        }
        public async Task DeleteHashTagByPostId(int postId)
        {
            var hashTags = _context.Hashtags.Where(h => h.Id == postId);
            _context.Hashtags.RemoveRange(hashTags);
            await _context.SaveChangesAsync();
        }
    }
}
