using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ActivityDraftRepository : IActivityDraftRepository
    {
        protected readonly EduShpereDbContext _context;
        protected readonly DbSet<ActivityDraft> _dbSet;

        public ActivityDraftRepository(EduShpereDbContext context)
        {
            _context = context;
            _dbSet = _context.ActivityDrafts;
        }

        public IQueryable<ActivityDraft> GetQueryable()
        {
            return _dbSet.Where(d => !d.IsDeleted);
        }

        public async Task<IEnumerable<ActivityDraft>> GetAllByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(d => d.CreatedBy == userId && !d.IsDeleted)
                .OrderByDescending(d => d.UpdatedAt)
                .ToListAsync();
        }

        public async Task<ActivityDraft?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<ActivityDraft?> GetByIdAndUserIdAsync(int id, int userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.Id == id && d.CreatedBy == userId && !d.IsDeleted);
        }

        public async Task AddAsync(ActivityDraft draft)
        {
            await _dbSet.AddAsync(draft);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ActivityDraft draft)
        {
            _dbSet.Update(draft);
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
    }
}

