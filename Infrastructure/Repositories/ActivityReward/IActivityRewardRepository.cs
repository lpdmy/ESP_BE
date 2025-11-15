using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivityRewardRepository : IRepository<ActivityReward>
{
    Task<IEnumerable<ActivityReward>> GetByActivityIdAsync(int activityId);
    Task DeleteByActivityIdAsync(int activityId);
}

