using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories;

public interface IActivityTemplateRepository
{
    Task<IEnumerable<ActivityTemplate>> GetAllAsync();
    Task<ActivityTemplate?> GetByIdAsync(int id);
    Task<IEnumerable<ActivityTemplate>> GetBySubTypeAsync(string subType);
    Task AddAsync(ActivityTemplate template);
    Task UpdateAsync(ActivityTemplate template);
    Task DeleteAsync(int id);
    Task IncrementUsageCountAsync(int id);
    IQueryable<ActivityTemplate> GetQueryable();
}

