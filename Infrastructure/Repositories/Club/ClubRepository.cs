using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ClubRepository : BaseRepository<Club>, IClubRepository
    {
        public ClubRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Club>> SearchAsync(string query, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Club>();
            }

            var normalizedQuery = query.Trim().ToLower();

            return await _context.Clubs
                .Where(c => !c.IsDeleted &&
                            ((c.Name ?? string.Empty).ToLower().Contains(normalizedQuery) ||
                             (c.Description ?? string.Empty).ToLower().Contains(normalizedQuery)))
                .Include(c => c.ClubMembers)
                .Include(c => c.CreatedByUser)
                .OrderByDescending(c => c.CreatedAt)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<Club?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Clubs
                .Where(c => c.Id == id && !c.IsDeleted)
                .Include(c => c.ClubMembers)
                .ThenInclude(m => m.User)
                .Include(c => c.Category)
                .Include(c => c.CreatedByUser)
                .Include(c => c.President)
                .Include(c => c.Mentor)
                .Include(c => c.ClubJoinRequests)
                .FirstOrDefaultAsync();
        }
        public IQueryable<Club> GetAllWithIncludes()
        {
            return _context.Clubs
                .Where(c => !c.IsDeleted)
                .Include(c => c.Category)
                .Include(c => c.Mentor)
                .Include(c => c.President)
                .Include(c=>c.Mentor)
                .Include(c => c.ClubMembers)
                    .ThenInclude(m => m.User);
        }
        public async Task<bool> DeleteSoft(int id)
        {

            var entity = await _context.Clubs.FindAsync(id);
            if (entity == null || entity.IsDeleted)
            {
                return false;
            }
            entity.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
