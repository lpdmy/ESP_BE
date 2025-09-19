using AutoMapper;
using EduShpere.Application.DTOs.UserProfileDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories.TeacherProfile;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared.Constants;
using TeacherProfileEntity = EduShpere.Domain.Models.TeacherProfile;
using EduShpere.Infrastructure;

namespace EduShpere.Application.Services.TeacherProfileService
{
    public class TeacherProfileService : ITeacherProfileService
    {
        private readonly ITeacherProfileRepository _teacherProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public TeacherProfileService(
            ITeacherProfileRepository teacherProfileRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _teacherProfileRepository = teacherProfileRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<GetTeacherProfileDto?> GetTeacherProfileByIdAsync(int id)
        {
            var profile = await _teacherProfileRepository.GetByIdAsync(id);
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
            var profile = _mapper.Map<TeacherProfileEntity>(dto);
            var createdProfile = await _teacherProfileRepository.CreateTeacherProfileAsync(profile);
            return _mapper.Map<TeacherProfileDto>(createdProfile);
        }

        public async Task<TeacherProfileDto> UpdateTeacherProfileAsync(int id, CreateUpdateTeacherProfileDto dto)
        {
            var exists = await _teacherProfileRepository.TeacherProfileExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);
            }

            var profile = _mapper.Map<TeacherProfileEntity>(dto);
            profile.Id = id;
            var updatedProfile = await _teacherProfileRepository.UpdateTeacherProfileAsync(profile);
            return _mapper.Map<TeacherProfileDto>(updatedProfile);
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

            await _userRepository.UpdateAsync(user);

            // Cập nhật thông tin TeacherProfile nếu có
            var profile = await _teacherProfileRepository.GetTeacherProfileByUserIdAsync(userId);
            if (profile != null)
            {
                if (!string.IsNullOrEmpty(dto.Bio))
                    profile.Bio = dto.Bio;
                if (!string.IsNullOrEmpty(dto.ExtraJson))
                    profile.ExtraJson = dto.ExtraJson;

                profile.UpdatedAt = DateTime.UtcNow;
                profile.UpdatedBy = userId;

                await _teacherProfileRepository.UpdateAsync(profile);
            }
            return true;
        }

        public async Task<TeacherProfileDto> UpdateTeacherInfoAsync(int id, UpdateTeacherInfoDto dto)
        {
            var exists = await _teacherProfileRepository.TeacherProfileExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException(ErrorMessages.UserProfile.ProfileNotFound);
            }

            var profile = _mapper.Map<TeacherProfileEntity>(dto);
            profile.Id = id;
            var teacher = await _teacherProfileRepository.UpdateTeacherProfileAsync(profile, dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            return _mapper.Map<TeacherProfileDto>(teacher);
        }
    }
}
