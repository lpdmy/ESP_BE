
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
    }
}
