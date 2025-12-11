using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivityProgramRepository : IRepository<ActivityProgram>
{
    Task<IEnumerable<ActivityProgram>> GetByActivityIdAsync(int activityId);
    Task DeleteByActivityIdAsync(int activityId);
}

