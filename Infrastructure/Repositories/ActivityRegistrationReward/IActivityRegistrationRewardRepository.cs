using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivityRegistrationRewardRepository : IRepository<ActivityRegistrationReward>
{
    Task<ActivityRegistrationReward?> GetByActivityIdAsync(int activityId);
    Task DeleteByActivityIdAsync(int activityId);
}

