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
                .FirstOrDefaultAsync(c => c.Name == name);
        }
        public IQueryable<FavoriteCollection> GetByUserIdQuery(User user)
        {
            return _context.FavoriteCollections.
                Include(c => c.CollectionItems)
                .Where(c => c.UserId == user.Id);
        }
    }
}
