using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivityRuleRepository : IRepository<ActivityRule>
{
    Task<IEnumerable<ActivityRule>> GetByActivityIdAsync(int activityId);
    Task DeleteByActivityIdAsync(int activityId);
}

