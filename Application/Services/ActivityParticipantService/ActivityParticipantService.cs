
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class ActivityParticipantService : IActivityParticipantService
    {
        private readonly IActivityParticipantRepository _repo;
        private readonly IActivityRepository _activityRepo;
        private readonly IClassGroupRepository _classGroupRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        public ActivityParticipantService(
            IActivityParticipantRepository repo, 
            IActivityRepository activityRepo,
            IClassGroupRepository classGroupRepo,
            IUserRepository userRepo,
            IMapper mapper) {
            _repo = repo;
            _activityRepo = activityRepo;
            _classGroupRepo = classGroupRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }
        public async Task<ActivityParticipantResponseDto> AddActivityParticipant(AddParticipantDto dto) {
            if (dto.UserId == null || dto.ActivityId == null) {
                throw new BadRequestException("Dữ liệu đầu vào không được rỗng");
            }
            
            // Kiểm tra đã đăng ký chưa
            bool isJoin = await _repo.isAlreadyRegistered(dto.UserId, dto.ActivityId);
            if (isJoin == true) {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.AlreadyJoined);
            }

            // Lấy thông tin Activity
            var activity = await _activityRepo.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Nếu Activity yêu cầu chỉ giáo viên chủ nhiệm mới được đăng ký
            if (activity.OnlyTeacherCanRegister == true)
            {
                // Lấy thông tin User để kiểm tra Role
                var user = await _userRepo.GetByIdAsync(dto.UserId);
                if (user == null)
                {
                    throw new NotFoundException(ErrorMessages.ActivityParticipant.TeacherNotFound);
                }

                // Kiểm tra User có phải là giáo viên không
                if (user.Role != UserRole.Teacher)
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.OnlyHomeroomTeacherCanRegister);
                }

                // Lấy niên khóa hiện tại (isCurrent = true)
                var currentAcademicYear = await _classGroupRepo.GetCurrentAcademicYearAsync();
                if (currentAcademicYear == null)
                {
                    throw new BadRequestException("Không tìm thấy niên khóa hiện tại. Vui lòng liên hệ quản trị viên.");
                }

                // Kiểm tra giáo viên có phải là giáo viên chủ nhiệm của lớp nào trong niên khóa hiện tại không
                var homeroomClass = await _classGroupRepo.GetTeacherCurrentHomeroomClassInSameAcademicYearAsync(dto.UserId, currentAcademicYear.Id);
                if (homeroomClass == null)
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.NotHomeroomTeacher);
                }

                // Nếu giáo viên là chủ nhiệm, tự động set ClassGroupId (nếu chưa có)
                if (!dto.ClassGroupId.HasValue)
                {
                    dto.ClassGroupId = homeroomClass.Id;
                }
                else
                {
                    // Nếu có ClassGroupId, kiểm tra giáo viên có phải là chủ nhiệm của lớp đó không
                    if (dto.ClassGroupId.Value != homeroomClass.Id)
                    {
                        // Kiểm tra xem giáo viên có phải chủ nhiệm của lớp được chỉ định không
                        bool isHomeroomOfSpecifiedClass = await _classGroupRepo.IsTeacherHomeroomOfClassGroupAsync(dto.UserId, dto.ClassGroupId.Value);
                        if (!isHomeroomOfSpecifiedClass)
                        {
                            throw new BadRequestException(ErrorMessages.ActivityParticipant.NotHomeroomTeacher);
                        }
                    }
                }
            }
            else
            {
                // Nếu Activity không yêu cầu OnlyTeacherCanRegister, có thể có ClassGroupId hoặc không
                // Nếu có ClassGroupId, kiểm tra lớp tồn tại và giáo viên có phải chủ nhiệm không
                if (dto.ClassGroupId.HasValue)
                {
                    var classGroup = await _classGroupRepo.GetClassGroupByIdWithAcademicYearAsync(dto.ClassGroupId.Value);
                    if (classGroup == null)
                    {
                        throw new BadRequestException(ErrorMessages.ActivityParticipant.InvalidClassGroup);
                    }

                    var user = await _userRepo.GetByIdAsync(dto.UserId);
                    if (user != null && user.Role == UserRole.Teacher)
                    {
                        // Nếu là giáo viên, kiểm tra có phải chủ nhiệm không
                        bool isHomeroomTeacher = await _classGroupRepo.IsTeacherHomeroomOfClassGroupAsync(dto.UserId, dto.ClassGroupId.Value);
                        if (!isHomeroomTeacher)
                        {
                            throw new BadRequestException(ErrorMessages.ActivityParticipant.NotHomeroomTeacher);
                        }
                    }
                }
            }

            var participant = new ActivityParticipant
            {
                ActivityId = dto.ActivityId,
                UserId = dto.UserId,
                ClassGroupId = dto.ClassGroupId,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                CreatedBy = dto.UserId,
                Status = ParticipantStatus.Joined,
                
            };
            await _repo.AddAsync(participant);
            var participantDto = _mapper.Map<ActivityParticipantResponseDto>(participant);
            return participantDto;
        }
        public async Task<ActivityParticipantResponseDto> RemoveActivityParticipant(int participationId)
        {
            var participant = await _repo.GetByIdAsync(participationId);
            if (participant == null)
            {
                throw new NotFoundException(ErrorMessages.ActivityParticipant.NotFound);
            }
            await _repo.SoftDeleteAsync(participationId);
            var participantDto = _mapper.Map<ActivityParticipantResponseDto>(participant);
            return participantDto;
        }
        public async Task<int> CountNumberParticipantInActivity(int activityId)
        {
            return await _repo.CountNumberParticipantInActivity(activityId);
        }

    }
}
