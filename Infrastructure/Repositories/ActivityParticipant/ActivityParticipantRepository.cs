

using System.Linq;
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
        public async Task<bool> IsAlreadyRegisteredAsync(int userId, int activityId, int? sportId = null)
        {
            var query = _dbSet.Where(p => p.UserId == userId && p.ActivityId == activityId && !p.IsDeleted);

            if (sportId.HasValue)
            {
                query = query.Where(p => p.SportId == sportId);
            }

            return await query.AnyAsync();
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
