using EduShpere.Application.DTOs.UserProfileDto;

namespace EduShpere.Application.Services.TeacherProfileService
{
    public interface ITeacherProfileService
    {
        Task<GetTeacherProfileDto?> GetTeacherProfileByIdAsync(int id);
        Task<GetTeacherProfileDto?> GetTeacherProfileByUserIdAsync(int userId);
        Task<IEnumerable<GetTeacherProfileDto>> GetAllTeacherProfilesAsync();
        Task<TeacherProfileDto> CreateTeacherProfileAsync(CreateUpdateTeacherProfileDto dto);
        Task<TeacherProfileDto> UpdateTeacherProfileAsync(int id, CreateUpdateTeacherProfileDto dto);
        Task<bool> DeleteTeacherProfileAsync(int id);
        Task<bool> TeacherProfileExistsAsync(int id);
        Task<bool> TeacherProfileExistsByUserIdAsync(int userId);
        
        // Personal Info Update (User only)
        Task<bool> UpdatePersonalInfoAsync(int userId, UpdatePersonalInfoDto dto);
        
        // Teacher Info Update (Admin only)
        Task<TeacherProfileDto> UpdateTeacherInfoAsync(int id, UpdateTeacherInfoDto dto);
    }
}
