using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class CollectionRepository : BaseRepository<FavoriteCollection> , ICollectionRepository
    {
        public CollectionRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<FavoriteCollection?> GetByNameAsync(string name)
        {
            return await _context.FavoriteCollections
                .FirstOrDefaultAsync(c => c.Name == name && c.IsDeleted == false);
        }
        public IQueryable<FavoriteCollection> GetByUserIdQuery(User user)
        {
            return _context.FavoriteCollections.
                Include(c => c.CollectionItems)
                .ThenInclude(ci => ci.Post)
                .ThenInclude(p => p.Attachments)
                .Where(c => c.UserId == user.Id && c.IsDeleted == false);
        }
        public async Task DeleteSoft(int id)
        {
            var collection = await GetByIdAsync(id);
            if (collection != null)
            {
                collection.IsDeleted = true;
                collection.UpdatedAt = DateTime.UtcNow;
                await UpdateAsync(collection);
            }
        }
    }
}
