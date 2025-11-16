using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivityRuleRepository : BaseRepository<ActivityRule>, IActivityRuleRepository
{
    public ActivityRuleRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivityRule>> GetByActivityIdAsync(int activityId)
    {
        return await _dbSet
            .Where(rule => rule.ActivityId == activityId && !rule.IsDeleted)
            .ToListAsync();
    }

    public async Task DeleteByActivityIdAsync(int activityId)
    {
        var rules = await _dbSet
            .Where(rule => rule.ActivityId == activityId)
            .ToListAsync();

        if (rules.Count > 0)
        {
            _dbSet.RemoveRange(rules);
        }
    }
}

