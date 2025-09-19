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
    }
}
