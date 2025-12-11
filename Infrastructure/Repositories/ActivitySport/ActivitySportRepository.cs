using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivitySportRepository : BaseRepository<ActivitySport>, IActivitySportRepository
{
    public ActivitySportRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivitySport>> GetByActivityIdAsync(int activityId)
    {
        return await _dbSet
            .Where(sport => sport.ActivityId == activityId && !sport.IsDeleted)
            .ToListAsync();
    }

    public async Task DeleteByActivityIdAsync(int activityId)
    {
        var sports = await _dbSet
            .Where(sport => sport.ActivityId == activityId)
            .ToListAsync();

        if (sports.Count > 0)
        {
            _dbSet.RemoveRange(sports);
        }
    }
}

