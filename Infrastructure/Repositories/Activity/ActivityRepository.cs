using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        protected readonly EduShpereDbContext _context;
        protected readonly DbSet<Activity> _dbSet;

        public ActivityRepository(EduShpereDbContext context)
        {
            _context = context;
            _dbSet = _context.Activities;
        }

        // Basic CRUD operations
        public async Task<IEnumerable<Activity>> GetAllAsync()
        {
            return await _dbSet
                .Where(a => !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task AddAsync(Activity activity)
        {
            await _dbSet.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Activity activity)
        {
            _dbSet.Update(activity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await UpdateAsync(entity);
            }
        }

        public async Task AddRangeAsync(IEnumerable<Activity> activities)
        {
            await _dbSet.AddRangeAsync(activities);
            await _context.SaveChangesAsync();
        }
        public async Task<Activity?> GetByIdWithIncludesAsync(int id)
        {
            var activity = await _context.Activities
                .Include(a => a.Rules.Where(r => !r.IsDeleted))
                .Include(a => a.ActivityParticipants.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.User)
                .Include(a => a.ActivityParticipants.Where(p => !p.IsDeleted))
                    .ThenInclude(p => p.ClassGroup)
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

        public async Task<(IEnumerable<Activity> Items, int TotalCount)> GetAllWithPagingAsync(int pageNumber, int pageSize, string? search = null)
        {
            // Base query - filter deleted items
            var query = _dbSet.Where(a => !a.IsDeleted);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                query = query.Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchLower))
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            try
            {
                // Apply pagination and ordering
                // Note: For list view, we don't need all navigation properties to reduce response size
                // Full details will be loaded when getting by ID
                var items = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, totalCount);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (null, totalCount);

            }


        }

        public async Task<(IEnumerable<Activity> Items, int TotalCount)> GetActivitiesByUserIdAsync(int userId, int pageNumber, int pageSize, string? search = null, string? status = null)
        {
            var now = DateTime.UtcNow;
            
            // Base query - join with ActivityParticipant to filter by userId
            var query = _context.Activities
                .Where(a => !a.IsDeleted)
                .Where(a => a.ActivityParticipants.Any(ap => ap.UserId == userId && !ap.IsDeleted));

            // Apply status filter if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusLower = status.ToLower().Trim();
                if (statusLower == "ongoing")
                {
                    // Ongoing: EndDate is null OR EndDate > now (includes upcoming and currently running)
                    query = query.Where(a => a.EndDate == null || a.EndDate > now);
                }
                else if (statusLower == "finished")
                {
                    // Finished: EndDate is not null AND EndDate <= now
                    query = query.Where(a => a.EndDate != null && a.EndDate <= now);
                }
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                query = query.Where(a => 
                    (a.Title != null && a.Title.ToLower().Contains(searchLower)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchLower))
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();
            
            try
            {
                // Apply pagination and ordering
                var items = await query
                    .Include(a => a.Rules.Where(r => !r.IsDeleted))
                    .Include(a => a.ActivityParticipants.Where(p => p.UserId == userId && !p.IsDeleted))
                    .Include(a => a.RegistrationReward)
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                
                return (items ?? new List<Activity>(), totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (new List<Activity>(), totalCount);
            }
        }
    }
}
