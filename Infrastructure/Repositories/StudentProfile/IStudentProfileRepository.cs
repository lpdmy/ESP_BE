using EduShpere.Domain;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IStudentProfileRepository
    {
        Task<StudentProfile?> GetStudentProfileByIdAsync(int id);
        Task<StudentProfile?> GetStudentProfileByUserIdAsync(int userId);
        Task<IEnumerable<StudentProfile>> GetAllStudentProfilesAsync();
        Task<StudentProfile> CreateStudentProfileAsync(StudentProfile profile, DateTime? birthDate = null, string? phoneNumber = null, string? avatarUrl = null, int? classGroupId = null);
        Task<StudentProfile> UpdateStudentProfileAsync(StudentProfile profile, DateTime? birthDate = null, string? phoneNumber = null, string? avatarUrl = null);
        Task<StudentProfile> AddAsync(StudentProfile profile);
        Task UpdateAsync(StudentProfile profile);
        Task<bool> DeleteStudentProfileAsync(int id);
        Task<bool> StudentProfileExistsAsync(int id);
        Task<bool> StudentProfileExistsByUserIdAsync(int userId);
    }
}
