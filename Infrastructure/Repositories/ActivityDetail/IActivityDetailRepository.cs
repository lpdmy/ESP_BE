using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivityDetailRepository : IRepository<ActivityDetail>
{
    Task<ActivityDetail?> GetByActivityIdAsync(int activityId);
    Task DeleteByActivityIdAsync(int activityId);
}

