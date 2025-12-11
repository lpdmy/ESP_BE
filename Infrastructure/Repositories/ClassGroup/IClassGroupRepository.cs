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
    Task<bool> IsExistsAsync(string name, int? grade, int? academicYearId, int? excludeId = null);
    Task<int> GetStudentCountAsync(int classGroupId);
    Task<IEnumerable<ClassGroup>> GetByFilterAsync(string? name = null, int? grade = null, int? academicYearId = null, bool? isDeleted = null);
    Task<IEnumerable<ClassGroup>> GetWithoutAcademicYearAsync();
    Task<IEnumerable<ClassGroup>> GetWithoutGradeAsync();
    
    // Student management operations
    Task<IEnumerable<User>> GetStudentsInClassAsync(int classGroupId);
    Task<bool> AddStudentToClassAsync(int classGroupId, int studentId);
    Task<bool> AddStudentToClassByEmailAsync(int classGroupId, string email);
    Task<bool> RemoveStudentFromClassAsync(int classGroupId, int studentId);
    Task<bool> IsStudentInClassAsync(int classGroupId, int studentId);
    Task<User?> GetHomeroomTeacherAsync(int classGroupId);
    Task<User?> GetStudentByEmailAsync(string email);
    Task<ClassGroup?> GetStudentCurrentClassAsync(int studentId);
    Task<ClassGroup?> GetStudentCurrentClassInAcademicYearAsync(int studentId, int academicStartYear);
    Task<ClassGroup?> GetStudentCurrentClassInSameAcademicYearAsync(int studentId, int academicYearId);
    Task<IEnumerable<AcademicYear>> GetAllAcademicYearsAsync();
    Task<AcademicYear?> GetCurrentAcademicYearAsync();
    
    // Current Class operations
    Task<ClassGroup?> GetCurrentClassByUserIdAsync(int userId);
    
    // Homeroom Teacher management operations
    Task<User?> GetTeacherByEmailAsync(string email);
    Task<bool> AssignHomeroomTeacherAsync(int classGroupId, int teacherId);
    Task<bool> RemoveHomeroomTeacherAsync(int classGroupId);
    Task<ClassGroup?> GetTeacherCurrentHomeroomClassInSameAcademicYearAsync(int teacherId, int academicYearId);
    Task<bool> IsTeacherHomeroomOfClassGroupAsync(int teacherId, int classGroupId);
    Task<ClassGroup?> GetClassGroupByIdWithAcademicYearAsync(int classGroupId);
}
