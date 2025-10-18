using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure;

public class ClassGroupRepository : IClassGroupRepository
{
    protected readonly EduShpereDbContext _context;
    protected readonly DbSet<ClassGroup> _dbSet;

    public ClassGroupRepository(EduShpereDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<ClassGroup>();
    }

    // Basic CRUD operations
    public async Task<IEnumerable<ClassGroup>> GetAllAsync()
    {
        return await _dbSet.Where(c => !c.IsDeleted).ToListAsync();
    }

    public async Task<ClassGroup?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task AddAsync(ClassGroup entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<ClassGroup> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClassGroup entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsDeleted = true; // Soft delete
            await UpdateAsync(entity);
        }
    }

    // Specific operations
    public async Task<IQueryable<ClassGroup>> GetQueryableAsync()
    {
        return await Task.FromResult(_dbSet
            .Where(c => !c.IsDeleted)
            .Include(c => c.ClassGroupMembers.Where(m => !m.IsDeleted)));
    }

    public async Task<ClassGroup?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted);
    }


    public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name == name && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> GetStudentCountAsync(int classGroupId)
    {
        return await _context.ClassGroupMembers
            .CountAsync(m => m.ClassGroupId == classGroupId && !m.IsDeleted);
    }

    public async Task<IEnumerable<ClassGroup>> GetByFilterAsync(string? name = null, int? grade = null, int? startYear = null, bool? isDeleted = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(c => c.Name!.Contains(name));
        }

        if (grade.HasValue)
        {
            query = query.Where(c => c.Grade == grade.Value);
        }

        if (startYear.HasValue)
        {
            query = query.Where(c => c.StartYear == startYear.Value);
        }

        if (isDeleted.HasValue)
        {
            query = query.Where(c => c.IsDeleted == isDeleted.Value);
        }
        else
        {
            query = query.Where(c => !c.IsDeleted);
        }

        return await query
            .Include(c => c.ClassGroupMembers.Where(m => !m.IsDeleted))
            .OrderBy(c => c.Grade)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }


    public async Task<IEnumerable<ClassGroup>> GetWithoutStartYearAsync()
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && !c.StartYear.HasValue)
            .Include(c => c.ClassGroupMembers.Where(m => !m.IsDeleted))
            .OrderBy(c => c.Grade)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ClassGroup>> GetWithoutGradeAsync()
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && !c.Grade.HasValue)
            .Include(c => c.ClassGroupMembers.Where(m => !m.IsDeleted))
            .OrderBy(c => c.StartYear)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
}
