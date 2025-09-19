using AutoMapper;
using EduShpere.Application.DTOs.UserProfileDto;
using EduShpere.Application.Services;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Repositories.TeacherProfile;
using TeacherProfileEntity = EduShpere.Domain.Models.TeacherProfile;
using EduShpere.Shared.Constants;

namespace EduShpere.Application
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly ITeacherProfileRepository _teacherProfileRepository;
        private readonly IMapper _mapper;
        private readonly IAuditService _auditService;
        
        public UserService(IUserRepository userRepository, IMapper mapper, IStudentProfileRepository studentProfileRepository, ITeacherProfileRepository teacherProfileRepository, IAuditService auditService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _studentProfileRepository = studentProfileRepository;
            _teacherProfileRepository = teacherProfileRepository;
            _auditService = auditService;
        }

        // User CRUD operations
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task AddUserAsync(User user)
        {
            _auditService.SetAuditFieldsForCreate(user);
            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            _auditService.SetAuditFieldsForUpdate(user);
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                _auditService.SetAuditFieldsForDelete(user);
                await _userRepository.UpdateAsync(user);
            }
        }

        // Student Profile CRUD operations
        public async Task<GetStudentProfileDto?> GetStudentProfileByIdAsync(int id)
        {
            var profile = await _studentProfileRepository.GetStudentProfileByIdAsync(id);
            return profile != null ? _mapper.Map<GetStudentProfileDto>(profile) : null;
        }

        public async Task<GetStudentProfileDto?> GetStudentProfileByUserIdAsync(int userId)
        {
            var profile = await _studentProfileRepository.GetStudentProfileByUserIdAsync(userId);
            return profile != null ? _mapper.Map<GetStudentProfileDto>(profile) : null;
        }

        public async Task<IEnumerable<GetStudentProfileDto>> GetAllStudentProfilesAsync()
        {
            var profiles = await _studentProfileRepository.GetAllStudentProfilesAsync();
            return _mapper.Map<IEnumerable<GetStudentProfileDto>>(profiles);
        }

        public async Task<StudentProfileDto> CreateStudentProfileAsync(CreateUpdateStudentProfileDto dto)
        {
            // Kiểm tra xem user đã có profile chưa
            var existingProfile = await _studentProfileRepository.StudentProfileExistsByUserIdAsync(dto.UserId);
            if (existingProfile)
            {
                throw new InvalidOperationException(ErrorMessages.UserProfile.ProfileAlreadyExists);
            }

            var profile = _mapper.Map<StudentProfile>(dto);
            _auditService.SetAuditFieldsForCreate(profile);
            var student = await _studentProfileRepository.CreateStudentProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl, dto.ClassGroupId);
            return _mapper.Map<StudentProfileDto>(student);
        }

        public async Task<StudentProfileDto> UpdateStudentProfileAsync(int id, CreateUpdateStudentProfileDto dto)
        {
            // Kiểm tra xem profile có tồn tại không
            var exists = await _studentProfileRepository.StudentProfileExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);
            }

            var profile = _mapper.Map<StudentProfile>(dto);
            profile.Id = id; // Đảm bảo cập nhật đúng profile
            _auditService.SetAuditFieldsForUpdate(profile);
            var student = await _studentProfileRepository.UpdateStudentProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            return _mapper.Map<StudentProfileDto>(student);
        }

        public async Task<bool> DeleteStudentProfileAsync(int id)
        {
            return await _studentProfileRepository.DeleteStudentProfileAsync(id);
        }

        public async Task<bool> StudentProfileExistsAsync(int id)
        {
            return await _studentProfileRepository.StudentProfileExistsAsync(id);
        }

        public async Task<bool> StudentProfileExistsByUserIdAsync(int userId)
        {
            return await _studentProfileRepository.StudentProfileExistsByUserIdAsync(userId);
        }

        // Personal Info Update (User only)
        public async Task<bool> UpdatePersonalInfoAsync(int userId, UpdatePersonalInfoDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.UserNotFound);
            }

            // Cập nhật thông tin User
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            if (dto.BirthDate != null)
                user.Birthdate = dto.BirthDate;

            if (!string.IsNullOrEmpty(dto.AvatarUrl))
                user.AvatarUrl = dto.AvatarUrl;

            _auditService.SetAuditFieldsForUpdate(user);
            await _userRepository.UpdateAsync(user);

            // Cập nhật thông tin StudentProfile nếu có
            var profile = await _studentProfileRepository.GetStudentProfileByUserIdAsync(userId);
            if (profile != null)
            {
                if (!string.IsNullOrEmpty(dto.Bio))
                    profile.Bio = dto.Bio;

                if (!string.IsNullOrEmpty(dto.ExtraJson))
                    profile.ExtraJson = dto.ExtraJson;

                _auditService.SetAuditFieldsForUpdate(profile, userId);
                await _studentProfileRepository.UpdateAsync(profile);
            }

            return true;
        }

        // Student Info Update (Admin only)
        public async Task<StudentProfileDto> UpdateStudentInfoAsync(int id, UpdateStudentInfoDto dto)
        {
            // Kiểm tra xem profile có tồn tại không
            var exists = await _studentProfileRepository.StudentProfileExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);
            }

            var profile = _mapper.Map<StudentProfile>(dto);
            profile.Id = id; // Đảm bảo cập nhật đúng profile
            _auditService.SetAuditFieldsForUpdate(profile);
            var student = await _studentProfileRepository.UpdateStudentProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            return _mapper.Map<StudentProfileDto>(student);
        }

        // Teacher Profile CRUD operations
        public async Task<GetTeacherProfileDto?> GetTeacherProfileByIdAsync(int id)
        {
            var profile = await _teacherProfileRepository.GetTeacherProfileByIdAsync(id);
            return profile != null ? _mapper.Map<GetTeacherProfileDto>(profile) : null;
        }

        public async Task<GetTeacherProfileDto?> GetTeacherProfileByUserIdAsync(int userId)
        {
            var profile = await _teacherProfileRepository.GetTeacherProfileByUserIdAsync(userId);
            return profile != null ? _mapper.Map<GetTeacherProfileDto>(profile) : null;
        }

        public async Task<IEnumerable<GetTeacherProfileDto>> GetAllTeacherProfilesAsync()
        {
            var profiles = await _teacherProfileRepository.GetAllTeacherProfilesAsync();
            return _mapper.Map<IEnumerable<GetTeacherProfileDto>>(profiles);
        }

        public async Task<TeacherProfileDto> CreateTeacherProfileAsync(CreateUpdateTeacherProfileDto dto)
        {
            // Kiểm tra xem user đã có profile chưa
            var existingProfile = await _teacherProfileRepository.TeacherProfileExistsByUserIdAsync(dto.UserId);
            if (existingProfile)
            {
                throw new InvalidOperationException(ErrorMessages.UserProfile.ProfileAlreadyExists);
            }

            var profile = _mapper.Map<TeacherProfileEntity>(dto);
            _auditService.SetAuditFieldsForCreate(profile);
            var teacher = await _teacherProfileRepository.CreateTeacherProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            return _mapper.Map<TeacherProfileDto>(teacher);
        }

        public async Task<TeacherProfileDto> UpdateTeacherProfileAsync(int id, CreateUpdateTeacherProfileDto dto)
        {
            // Kiểm tra xem profile có tồn tại không
            var exists = await _teacherProfileRepository.TeacherProfileExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);
            }

            var profile = _mapper.Map<TeacherProfileEntity>(dto);
            profile.Id = id; // Đảm bảo cập nhật đúng profile
            _auditService.SetAuditFieldsForUpdate(profile);
            var teacher = await _teacherProfileRepository.UpdateTeacherProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            return _mapper.Map<TeacherProfileDto>(teacher);
        }

        public async Task<bool> DeleteTeacherProfileAsync(int id)
        {
            return await _teacherProfileRepository.DeleteTeacherProfileAsync(id);
        }

        public async Task<bool> TeacherProfileExistsAsync(int id)
        {
            return await _teacherProfileRepository.TeacherProfileExistsAsync(id);
        }

        public async Task<bool> TeacherProfileExistsByUserIdAsync(int userId)
        {
            return await _teacherProfileRepository.TeacherProfileExistsByUserIdAsync(userId);
        }

        // Teacher Info Update (Admin only)
        public async Task<TeacherProfileDto> UpdateTeacherInfoAsync(int id, UpdateTeacherInfoDto dto)
        {
            // Kiểm tra xem profile có tồn tại không
            var exists = await _teacherProfileRepository.TeacherProfileExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);
            }

            var profile = _mapper.Map<TeacherProfileEntity>(dto);
            profile.Id = id; // Đảm bảo cập nhật đúng profile
            _auditService.SetAuditFieldsForUpdate(profile);
            var teacher = await _teacherProfileRepository.UpdateTeacherProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            return _mapper.Map<TeacherProfileDto>(teacher);
        }
    }
}