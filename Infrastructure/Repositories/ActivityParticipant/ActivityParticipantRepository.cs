

using System.Numerics;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ActivityParticipantRepository : BaseRepository<ActivityParticipant>, IActivityParticipantRepository
    {
        public ActivityParticipantRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<bool> isAlreadyRegistered(int UserId, int ActivityId) { 
          return await _dbSet.AnyAsync(p => p.UserId == UserId && p.ActivityId == ActivityId && p.IsDeleted == false);
        }
         public async Task SoftDeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.Now;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<int> CountNumberParticipantInActivity(int activityId)
        {
            var count = await _dbSet.Where(ap => ap.ActivityId == activityId && !ap.IsDeleted).CountAsync();
            return count;
        }

    }
}
