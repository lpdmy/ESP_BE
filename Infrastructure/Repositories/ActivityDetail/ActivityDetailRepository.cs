using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivityDetailRepository : BaseRepository<ActivityDetail>, IActivityDetailRepository
{
    public ActivityDetailRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<ActivityDetail?> GetByActivityIdAsync(int activityId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(detail => detail.ActivityId == activityId && !detail.IsDeleted);
    }

    public async Task DeleteByActivityIdAsync(int activityId)
    {
        var detail = await _dbSet
            .FirstOrDefaultAsync(d => d.ActivityId == activityId);

        if (detail != null)
        {
            _dbSet.Remove(detail);
        }
    }
}

