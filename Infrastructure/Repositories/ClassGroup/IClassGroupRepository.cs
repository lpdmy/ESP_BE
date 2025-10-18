using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure;

public interface IClassGroupRepository
{
    // Basic CRUD operations
    Task<IEnumerable<ClassGroup>> GetAllAsync();
    Task<ClassGroup?> GetByIdAsync(int id);
    Task AddAsync(ClassGroup entity);
    Task AddRangeAsync(IEnumerable<ClassGroup> entities);
    Task UpdateAsync(ClassGroup entity);
    Task DeleteAsync(int id);
    
    // Specific operations
    Task<IQueryable<ClassGroup>> GetQueryableAsync();
    Task<ClassGroup?> GetByNameAsync(string name);
    Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
    Task<int> GetStudentCountAsync(int classGroupId);
    Task<IEnumerable<ClassGroup>> GetByFilterAsync(string? name = null, int? grade = null, int? startYear = null, bool? isDeleted = null);
    Task<IEnumerable<ClassGroup>> GetWithoutStartYearAsync();
    Task<IEnumerable<ClassGroup>> GetWithoutGradeAsync();
}
