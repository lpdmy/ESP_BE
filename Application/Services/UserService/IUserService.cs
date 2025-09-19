using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.UserProfileDto;
using EduShpere.Domain;
using EduShpere.Domain.Models;

namespace EduShpere.Application
{
    public interface IUserService
    {
        // User CRUD operations
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);

        // Student Profile CRUD operations
        Task<GetStudentProfileDto?> GetStudentProfileByIdAsync(int id);
        Task<GetStudentProfileDto?> GetStudentProfileByUserIdAsync(int userId);
        Task<IEnumerable<GetStudentProfileDto>> GetAllStudentProfilesAsync();
        Task<StudentProfileDto> CreateStudentProfileAsync(CreateUpdateStudentProfileDto dto);
        Task<StudentProfileDto> UpdateStudentProfileAsync(int id, CreateUpdateStudentProfileDto dto);
        Task<bool> DeleteStudentProfileAsync(int id);
        Task<bool> StudentProfileExistsAsync(int id);
        Task<bool> StudentProfileExistsByUserIdAsync(int userId);
        
        // Personal Info Update (User only)
        Task<bool> UpdatePersonalInfoAsync(int userId, UpdatePersonalInfoDto dto);
        
        // Student Info Update (Admin only)
        Task<StudentProfileDto> UpdateStudentInfoAsync(int id, UpdateStudentInfoDto dto);
        
        // Teacher Profile CRUD operations
        Task<GetTeacherProfileDto?> GetTeacherProfileByIdAsync(int id);
        Task<GetTeacherProfileDto?> GetTeacherProfileByUserIdAsync(int userId);
        Task<IEnumerable<GetTeacherProfileDto>> GetAllTeacherProfilesAsync();
        Task<TeacherProfileDto> CreateTeacherProfileAsync(CreateUpdateTeacherProfileDto dto);
        Task<TeacherProfileDto> UpdateTeacherProfileAsync(int id, CreateUpdateTeacherProfileDto dto);
        Task<bool> DeleteTeacherProfileAsync(int id);
        Task<bool> TeacherProfileExistsAsync(int id);
        Task<bool> TeacherProfileExistsByUserIdAsync(int userId);
        
        // Teacher Info Update (Admin only)
        Task<TeacherProfileDto> UpdateTeacherInfoAsync(int id, UpdateTeacherInfoDto dto);
    }
}
