

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
    }
}
