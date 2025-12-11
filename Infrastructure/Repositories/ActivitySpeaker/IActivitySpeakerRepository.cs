using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivitySpeakerRepository : IRepository<ActivitySpeaker>
{
    Task<IEnumerable<ActivitySpeaker>> GetByActivityIdAsync(int activityId);
    Task DeleteByActivityIdAsync(int activityId);
}

