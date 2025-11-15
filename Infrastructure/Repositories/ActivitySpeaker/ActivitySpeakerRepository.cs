using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivitySpeakerRepository : BaseRepository<ActivitySpeaker>, IActivitySpeakerRepository
{
    public ActivitySpeakerRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivitySpeaker>> GetByActivityIdAsync(int activityId)
    {
        return await _dbSet
            .Where(speaker => speaker.ActivityId == activityId && !speaker.IsDeleted)
            .OrderBy(speaker => speaker.Order)
            .ToListAsync();
    }

    public async Task DeleteByActivityIdAsync(int activityId)
    {
        var speakers = await _dbSet
            .Where(speaker => speaker.ActivityId == activityId)
            .ToListAsync();

        if (speakers.Count > 0)
        {
            _dbSet.RemoveRange(speakers);
        }
    }
}

