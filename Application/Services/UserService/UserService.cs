using AutoMapper;
using EduShpere.Application.DTOs.UserProfileDto;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;

namespace EduShpere.Application
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentProfileRepository _studentProfileRepository;
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository, IMapper mapper, IStudentProfileRepository studentProfileRepository)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _studentProfileRepository = studentProfileRepository;
        }
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
            await _userRepository.AddAsync(user);
        }
        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }
        public async Task DeleteUserAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<StudentProfileDto> CreateOrUpdateStudentProfileAsync(CreateUpdateStudentProfileDto dto)
        {
            var profile = _mapper.Map<StudentProfile>(dto);
            var student = await _studentProfileRepository.CreateOrUpdateStudentProfileAsync(profile,  dto.BirthDate, dto.PhoneNumber, dto.AvatarUrl);
            return _mapper.Map<StudentProfileDto>(student);
        }
    }
}
