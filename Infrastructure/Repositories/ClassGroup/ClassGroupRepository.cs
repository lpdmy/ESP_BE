using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Domain.Enum;
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
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .Include(c => c.ClassGroupMembers.Where(m => !m.IsDeleted))
            .Include(c => c.Teacher)
            .Include(c => c.AcademicYears)
            .ToListAsync();
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
            entity.IsDeleted = true;
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

    public async Task<bool> IsExistsAsync(string name, int? grade, int? academicYearId, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name == name && !c.IsDeleted);
        if (grade.HasValue)
        {
            query = query.Where(c => c.Grade == grade.Value);
        }
        if (academicYearId.HasValue)
        {
            query = query.Where(c => c.AcademicYearId == academicYearId.Value);
        }
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

    public async Task<IEnumerable<ClassGroup>> GetByFilterAsync(string? name = null, int? grade = null, int? academicYearId = null, bool? isDeleted = null)
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

        if (academicYearId.HasValue)
        {
            query = query.Where(c => c.AcademicYearId == academicYearId.Value);
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
            .Include(c => c.Teacher)
            .Include(c => c.AcademicYears)
            .OrderBy(c => c.Grade)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }


    public async Task<IEnumerable<ClassGroup>> GetWithoutAcademicYearAsync()
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && !c.AcademicYearId.HasValue)
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
            .OrderBy(c => c.AcademicYearId)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    // Student management operations
    public async Task<IEnumerable<User>> GetStudentsInClassAsync(int classGroupId)
    {
        return await _context.ClassGroupMembers
            .Where(m => m.ClassGroupId == classGroupId && !m.IsDeleted)
            .Include(m => m.User)
            .Select(m => m.User)
            .Where(u => u.Role == UserRole.Student)
            .ToListAsync();
    }

    public async Task<bool> AddStudentToClassAsync(int classGroupId, int studentId)
    {
        // Check if student is already in class
        var existingMember = await _context.ClassGroupMembers
            .FirstOrDefaultAsync(m => m.ClassGroupId == classGroupId && m.UserId == studentId && !m.IsDeleted);

        if (existingMember != null)
            return false; // Student already in class

        // Check if student exists and is a student
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student && !u.IsDeleted);

        if (student == null)
            return false; // Student not found or not a student

        var member = new ClassGroupMember
        {
            ClassGroupId = classGroupId,
            UserId = studentId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _context.ClassGroupMembers.AddAsync(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveStudentFromClassAsync(int classGroupId, int studentId)
    {
        var member = await _context.ClassGroupMembers
            .FirstOrDefaultAsync(m => m.ClassGroupId == classGroupId && m.UserId == studentId && !m.IsDeleted);

        if (member == null)
            return false;

        member.IsDeleted = true;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsStudentInClassAsync(int classGroupId, int studentId)
    {
        return await _context.ClassGroupMembers
            .AnyAsync(m => m.ClassGroupId == classGroupId && m.UserId == studentId && !m.IsDeleted);
    }

    public async Task<User?> GetStudentByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Role == UserRole.Student && !u.IsDeleted);
    }

    public async Task<ClassGroup?> GetStudentCurrentClassAsync(int studentId)
    {
        return await _context.ClassGroupMembers
            .Where(m => m.UserId == studentId && !m.IsDeleted)
            .Include(m => m.ClassGroup)
            .Select(m => m.ClassGroup)
            .FirstOrDefaultAsync();
    }

    public async Task<ClassGroup?> GetStudentCurrentClassInAcademicYearAsync(int studentId, int academicYearId)
    {
        return await _context.ClassGroupMembers
            .Where(m => m.UserId == studentId && !m.IsDeleted)
            .Include(m => m.ClassGroup)
            .Where(m => m.ClassGroup.AcademicYearId == academicYearId)
            .Select(m => m.ClassGroup)
            .FirstOrDefaultAsync();
    }

    public async Task<ClassGroup?> GetStudentCurrentClassInSameAcademicYearAsync(int studentId, int academicYearId)
    {
        // Get all classes the student is currently in with the same academic year (regardless of grade)
        return await _context.ClassGroupMembers
            .Where(m => m.UserId == studentId && !m.IsDeleted)
            .Include(m => m.ClassGroup)
            .Where(m => m.ClassGroup.AcademicYearId == academicYearId && 
                       !m.ClassGroup.IsDeleted)
            .Select(m => m.ClassGroup)
            .FirstOrDefaultAsync();
    }


    public async Task<bool> AddStudentToClassByEmailAsync(int classGroupId, string email)
    {
        // Find student by email
        var student = await GetStudentByEmailAsync(email);
        if (student == null)
            return false; // Student not found

        // Check if student is already in any class
        var currentClass = await GetStudentCurrentClassAsync(student.Id);
        if (currentClass != null)
            return false; // Student already in a class

        // Add student to the specified class
        return await AddStudentToClassAsync(classGroupId, student.Id);
    }

    public async Task<IEnumerable<AcademicYear>> GetAllAcademicYearsAsync()
    {
        return await _context.AcademicYears
            .OrderByDescending(ay => ay.StartDate)
            .ToListAsync();
    }

    public async Task<User?> GetTeacherByEmailAsync(string email)
    {
        return await _context.Users
            .Where(u => u.Email == email && u.Role == UserRole.Teacher)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AssignHomeroomTeacherAsync(int classGroupId, int teacherId)
    {
        var classGroup = await _context.ClassGroups.FindAsync(classGroupId);
        if (classGroup == null)
            return false;

        classGroup.TeacherId = teacherId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveHomeroomTeacherAsync(int classGroupId)
    {
        var classGroup = await _context.ClassGroups.FindAsync(classGroupId);
        if (classGroup == null)
            return false;

        classGroup.TeacherId = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetHomeroomTeacherAsync(int classGroupId)
    {
        return await _context.ClassGroups
            .Where(cg => cg.Id == classGroupId && !cg.IsDeleted)
            .Include(cg => cg.Teacher)
            .Select(cg => cg.Teacher)
            .FirstOrDefaultAsync();
    }

    public async Task<ClassGroup?> GetTeacherCurrentHomeroomClassInSameAcademicYearAsync(int teacherId, int academicYearId)
    {
        return await _context.ClassGroups
            .Where(cg => cg.TeacherId == teacherId && 
                        cg.AcademicYearId == academicYearId && 
                        !cg.IsDeleted)
            .FirstOrDefaultAsync();
    }
}
