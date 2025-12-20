using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories;

public class ActivityTemplateRepository : BaseRepository<ActivityTemplate>, IActivityTemplateRepository
{
    public ActivityTemplateRepository(EduShpereDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ActivityTemplate>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.UsageCount)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }
        catch (Exception)
        {
            // Trả về empty list nếu bảng chưa tồn tại
            return new List<ActivityTemplate>();
        }
    }

    public async Task<ActivityTemplate?> GetByIdAsync(int id)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<IEnumerable<ActivityTemplate>> GetBySubTypeAsync(string subType)
    {
        return await _dbSet
            .Where(t => !t.IsDeleted && t.SubType == subType)
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task IncrementUsageCountAsync(int id)
    {
        var template = await GetByIdAsync(id);
        if (template != null)
        {
            template.UsageCount++;
            await UpdateAsync(template);
        }
    }

    public IQueryable<ActivityTemplate> GetQueryable()
    {
        return _dbSet.Where(t => !t.IsDeleted);
    }
}

