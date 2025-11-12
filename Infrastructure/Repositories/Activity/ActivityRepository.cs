using System;
using System.Collections.Generic;
using System.Linq;
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
            var activity = await _context.Activities
                .Include(a => a.Rules.Where(r => !r.IsDeleted))
                .Include(a => a.ActivityParticipants.Where(p => !p.IsDeleted))
                .Include(a => a.ActivityRewards.Where(r => !r.IsDeleted))
                .Include(a => a.Submissions.Where(s => !s.IsDeleted))
                .Include(a => a.Speakers.Where(s => !s.IsDeleted))
                .Include(a => a.Programs.Where(p => !p.IsDeleted))
                .Include(a => a.Sports.Where(s => !s.IsDeleted))
                .Include(a => a.ActivityDetail)
                .Include(a => a.RegistrationReward)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
            
            if (activity != null)
            {
                // Order collections after loading
                if (activity.Speakers != null)
                {
                    activity.Speakers = activity.Speakers.OrderBy(s => s.Order).ToList();
                }
                if (activity.Programs != null)
                {
                    activity.Programs = activity.Programs.OrderBy(p => p.Order).ToList();
                }
            }
            
            return activity;
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
