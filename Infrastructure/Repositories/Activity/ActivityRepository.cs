using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ActivityRepository : BaseRepository<Activity>, IActivityRepository
    {
        public ActivityRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<Activity?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Activities
                .Include(a => a.Rules)
                .Include(a => a.ActivityParticipants)
                .Include(a => a.ActivityRewards)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<IEnumerable<Activity>> SearchAsync(string query, int limit = 10)
        {
            var searchQuery = query.ToLower().Trim();

            return await _context.Activities
                .Include(a => a.CreatedByUser)
                .Include(a => a.ActivityParticipants)
                .Where(a => !a.IsDeleted)
                .Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchQuery)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchQuery)) ||
                    (a.Location != null && a.Location.ToLower().Contains(searchQuery)) ||
                    (a.Organizer != null && a.Organizer.ToLower().Contains(searchQuery))
                )
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }
    }
}
