using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivityProgramRepository : BaseRepository<ActivityProgram>, IActivityProgramRepository
{
    public ActivityProgramRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivityProgram>> GetByActivityIdAsync(int activityId)
    {
        return await _dbSet
            .Where(program => program.ActivityId == activityId && !program.IsDeleted)
            .OrderBy(program => program.Order)
            .ToListAsync();
    }

    public async Task DeleteByActivityIdAsync(int activityId)
    {
        var programs = await _dbSet
            .Where(program => program.ActivityId == activityId)
            .ToListAsync();

        if (programs.Count > 0)
        {
            _dbSet.RemoveRange(programs);
        }
    }
}

