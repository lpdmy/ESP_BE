using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class JuryActivityRepository : BaseRepository<JuryActivity>, IJuryActivityRepository
    {
        public JuryActivityRepository(EduShpereDbContext context) : base(context)
        {
        }
        public IQueryable<JuryActivity> GetAllJuryActivityIncluding()
        {
            return _dbSet
                .Include(ja => ja.User)
                .Include(ja => ja.Activity);
        }
        public IQueryable<JuryActivity> GetAllJuryActivityByActivityIdIncluding(int activityId)
        {
            return _dbSet
                .Where(ja => ja.ActivityId == activityId)
                .Include(ja => ja.User)
                .Include(ja => ja.Activity);
        }
        public async Task<bool> IsExisting(int userId,int activityId)
        {
            return await _dbSet.AnyAsync(ja => ja.UserId == userId && ja.ActivityId == activityId);
        }

    }
}
