using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivityRegistrationRewardRepository : BaseRepository<ActivityRegistrationReward>, IActivityRegistrationRewardRepository
{
    public ActivityRegistrationRewardRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<ActivityRegistrationReward?> GetByActivityIdAsync(int activityId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(reward => reward.ActivityId == activityId && !reward.IsDeleted);
    }

    public async Task DeleteByActivityIdAsync(int activityId)
    {
        var reward = await _dbSet
            .FirstOrDefaultAsync(r => r.ActivityId == activityId);

        if (reward != null)
        {
            _dbSet.Remove(reward);
        }
    }
}

