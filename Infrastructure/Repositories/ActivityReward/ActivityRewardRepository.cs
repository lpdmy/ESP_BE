using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivityRewardRepository : BaseRepository<ActivityReward>, IActivityRewardRepository
{
    public ActivityRewardRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivityReward>> GetByActivityIdAsync(int activityId)
    {
        return await _dbSet
            .Where(reward => reward.ActivityId == activityId && !reward.IsDeleted)
            .ToListAsync();
    }

    public async Task DeleteByActivityIdAsync(int activityId)
    {
        var rewards = await _dbSet
            .Where(reward => reward.ActivityId == activityId)
            .ToListAsync();

        if (rewards.Count > 0)
        {
            _dbSet.RemoveRange(rewards);
        }
    }
}

