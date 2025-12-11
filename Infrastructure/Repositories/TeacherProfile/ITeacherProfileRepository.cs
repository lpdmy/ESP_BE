using EduShpere.Domain.Models;
using TeacherProfileEntity = EduShpere.Domain.Models.TeacherProfile;

namespace EduShpere.Infrastructure.Repositories.TeacherProfile
{
    public interface ITeacherProfileRepository
    {
        Task<TeacherProfileEntity> AddAsync(TeacherProfileEntity profile);
        Task UpdateAsync(TeacherProfileEntity profile);
        Task<TeacherProfileEntity?> GetByIdAsync(int id);
        Task<TeacherProfileEntity?> GetTeacherProfileByIdAsync(int id);
        Task<TeacherProfileEntity?> GetTeacherProfileByUserIdAsync(int userId);
        Task<IEnumerable<TeacherProfileEntity>> GetAllTeacherProfilesAsync();
        Task<TeacherProfileEntity> CreateTeacherProfileAsync(TeacherProfileEntity profile, DateTime? birthDate = null, string? phoneNumber = null, string? avatarUrl = null);
        Task<TeacherProfileEntity> UpdateTeacherProfileAsync(TeacherProfileEntity profile, DateTime? birthDate = null, string? phoneNumber = null, string? avatarUrl = null);
        Task<bool> DeleteTeacherProfileAsync(int id);
        Task<bool> TeacherProfileExistsAsync(int id);
        Task<bool> TeacherProfileExistsByUserIdAsync(int userId);
    }
}