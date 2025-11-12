using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivitySportRepository : IRepository<ActivitySport>
{
    Task<IEnumerable<ActivitySport>> GetByActivityIdAsync(int activityId);
    Task DeleteByActivityIdAsync(int activityId);
}

