
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
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
        private readonly IActivitySportRepository _activitySportRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        private readonly EduShpereDbContext _context;
        private readonly IPaginationService _paginationService;
        private static readonly JsonSerializerOptions RegistrationJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public ActivityParticipantService(
            IActivityParticipantRepository repo, 
            IActivityRepository activityRepo,
            IClassGroupRepository classGroupRepo,
            IActivitySportRepository activitySportRepo,
            IUserRepository userRepo,
            IMapper mapper,
            EduShpereDbContext context,
            IPaginationService paginationService) {
            _repo = repo;
            _activityRepo = activityRepo;
            _classGroupRepo = classGroupRepo;
            _activitySportRepo = activitySportRepo;
            _userRepo = userRepo;
            _mapper = mapper;
            _context = context;
            _paginationService = paginationService;
        }
        public async Task<ActivityParticipantResponseDto> AddActivityParticipant(AddParticipantDto dto) {
            if (dto.UserId == null || dto.ActivityId == null) {
                throw new BadRequestException("Dữ liệu đầu vào không được rỗng");
            }
            
            // Kiểm tra đã đăng ký chưa
            bool isJoin = await _repo.IsAlreadyRegisteredAsync(dto.UserId, dto.ActivityId);
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
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                CreatedBy = dto.UserId,
                Status = ParticipantStatus.Joined,
                
            };
            await _repo.AddAsync(participant);
            var participantDto = _mapper.Map<ActivityParticipantResponseDto>(participant);
            return participantDto;
        }

        public async Task<GroupRegistrationResultDto> RegisterGroupAsync(GroupRegistrationDto dto)
        {
            if (dto.MemberIds == null || !dto.MemberIds.Any())
            {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.MembersRequired);
            }

            if (!dto.MemberIds.Contains(dto.LeaderId))
            {
                dto.MemberIds.Insert(0, dto.LeaderId);
            }

            var distinctMemberIds = dto.MemberIds.Distinct().ToList();

            var activity = await _activityRepo.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            if (!string.Equals(activity.SubType, "CreativeContest", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.ActivityNotSupportGroupRegistration);
            }

            var registrationSettings = GetRegistrationSettings(activity)?.GroupRegistration;
            if (registrationSettings != null)
            {
                if (distinctMemberIds.Count < registrationSettings.MinMembers)
                {
                    var maxMembersStr = registrationSettings.MaxMembers.HasValue ? registrationSettings.MaxMembers.Value.ToString() : "∞";
                    throw new BadRequestException(string.Format(ErrorMessages.ActivityParticipant.GroupSizeOutOfRange, registrationSettings.MinMembers, maxMembersStr));
                }
                if (registrationSettings.MaxMembers.HasValue && distinctMemberIds.Count > registrationSettings.MaxMembers.Value)
                {
                    throw new BadRequestException(string.Format(ErrorMessages.ActivityParticipant.GroupSizeOutOfRange, registrationSettings.MinMembers, registrationSettings.MaxMembers.Value));
                }
                if (registrationSettings.RequireLeader && dto.LeaderId <= 0)
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.LeaderRequired);
                }
            }

            // For CreativeContest, members can come from different classes (school-wide)
            // We only need to ensure the leader has a class, but members can be from anywhere
            var classGroupId = dto.ClassGroupId;
            if (!classGroupId.HasValue)
            {
                // Get leader's class for the group registration
                var leaderClass = await _classGroupRepo.GetStudentCurrentClassAsync(dto.LeaderId);
                if (leaderClass == null)
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.ClassGroupIdRequired);
                }
                classGroupId = leaderClass.Id;
            }
            else
            {
                var classGroup = await _classGroupRepo.GetByIdAsync(classGroupId.Value);
                if (classGroup == null || classGroup.IsDeleted)
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.InvalidClassGroup);
                }
            }

            // Validate each member
            foreach (var memberId in distinctMemberIds)
            {
                // For CreativeContest, members can be from different classes (school-wide registration)
                // We only validate that the member exists and is not already registered
                // The leader's class is used as the group's class, but members don't need to be in the same class
                
                // Check if member is already registered for this activity
                if (await _repo.IsAlreadyRegisteredAsync(memberId, dto.ActivityId))
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.AlreadyJoined);
                }
                
                // Note: We don't check IsStudentInClassAsync for CreativeContest
                // because members can come from anywhere in the school system
            }

            var now = DateTime.UtcNow;
            var groupCode = Guid.NewGuid();

            var participants = distinctMemberIds.Select(memberId => new ActivityParticipant
            {
                ActivityId = dto.ActivityId,
                UserId = memberId,
                ClassGroupId = classGroupId,
                GroupCode = groupCode,
                IsLeader = memberId == dto.LeaderId,
                RegistrationMetadata = dto.GroupName,
                CreatedAt = now,
                CreatedBy = dto.RequestedByUserId ?? dto.LeaderId,
                Status = ParticipantStatus.Joined,
                IsDeleted = false
            }).ToList();

            await _repo.AddRangeAsync(participants);

            return new GroupRegistrationResultDto
            {
                GroupCode = groupCode,
                Participants = _mapper.Map<IEnumerable<ActivityParticipantResponseDto>>(participants)
            };
        }

        public async Task<SportRegistrationResultDto> RegisterSportAsync(SportRegistrationDto dto)
        {
            if (dto.MemberIds == null || !dto.MemberIds.Any())
            {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.MembersRequired);
            }

            var distinctMemberIds = dto.MemberIds.Distinct().ToList();

            var activity = await _activityRepo.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            if (!string.Equals(activity.SubType, "SportsFestival", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.ActivityNotSupportSportRegistration);
            }

            var sport = await _activitySportRepo.GetByIdAsync(dto.SportId);
            if (sport == null || sport.ActivityId != dto.ActivityId)
            {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.SportNotFound);
            }

            var classGroup = await _classGroupRepo.GetByIdAsync(dto.ClassGroupId);
            if (classGroup == null || classGroup.IsDeleted)
            {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.InvalidClassGroup);
            }

            foreach (var memberId in distinctMemberIds)
            {
                if (!await _classGroupRepo.IsStudentInClassAsync(dto.ClassGroupId, memberId))
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.StudentNotInClass);
                }

                if (await _repo.IsAlreadyRegisteredAsync(memberId, dto.ActivityId, dto.SportId))
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.AlreadyJoined);
                }
            }

            if (sport.MaxMembers.HasValue)
            {
                var currentCount = await _context.ActivityParticipants
                    .Where(p => p.ActivityId == dto.ActivityId &&
                                p.SportId == dto.SportId &&
                                p.ClassGroupId == dto.ClassGroupId &&
                                !p.IsDeleted)
                    .CountAsync();

                if (currentCount + distinctMemberIds.Count > sport.MaxMembers.Value)
                {
                    throw new BadRequestException(ErrorMessages.ActivityParticipant.SportLimitExceeded);
                }
            }

            var now = DateTime.UtcNow;
            var participants = distinctMemberIds.Select(memberId => new ActivityParticipant
            {
                ActivityId = dto.ActivityId,
                UserId = memberId,
                ClassGroupId = dto.ClassGroupId,
                SportId = dto.SportId,
                CreatedAt = now,
                CreatedBy = dto.RequestedByUserId ?? memberId,
                Status = ParticipantStatus.Joined,
                IsDeleted = false
            }).ToList();

            await _repo.AddRangeAsync(participants);

            return new SportRegistrationResultDto
            {
                SportId = dto.SportId,
                ClassGroupId = dto.ClassGroupId,
                Participants = _mapper.Map<IEnumerable<ActivityParticipantResponseDto>>(participants)
            };
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

        public async Task<bool> CancelRegistrationAsync(int activityId, int userId)
        {
            // Lấy activity để kiểm tra thời hạn đăng ký
            var activity = await _activityRepo.GetByIdAsync(activityId);
            if (activity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Kiểm tra thời hạn đăng ký - chỉ cho phép hủy trước khi hết hạn đăng ký
            var now = DateTime.UtcNow;
            if (now > activity.EndRegisterDate)
            {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.CannotCancelAfterDeadline);
            }

            // Tìm tất cả participants của user trong activity này
            var participants = await _repo.GetByActivityIdAndUserIdAsync(activityId, userId);
            if (participants == null || !participants.Any())
            {
                throw new NotFoundException(ErrorMessages.ActivityParticipant.NotFound);
            }

            // Soft delete tất cả participants của user (bao gồm cả group và sport registrations)
            foreach (var participant in participants)
            {
                await _repo.SoftDeleteAsync(participant.Id);
            }

            return true;
        }

        private ActivityRegistrationSettingsDto? GetRegistrationSettings(Activity activity)
        {
            if (string.IsNullOrWhiteSpace(activity.RegistrationSettings))
            {
                // Return default settings based on SubType
                return CreateDefaultRegistrationSettings(activity.SubType);
            }

            try
            {
                var settings = JsonSerializer.Deserialize<ActivityRegistrationSettingsDto>(activity.RegistrationSettings, RegistrationJsonOptions);
                // Ensure GroupRegistration has default values if missing
                if (settings != null && settings.GroupRegistration == null)
                {
                    settings.GroupRegistration = new GroupRegistrationSettingsDto
                    {
                        MinMembers = 1,
                        MaxMembers = null,
                        RequireLeader = activity.SubType == "CreativeContest"
                    };
                }
                // Ensure MinMembers has default if not set
                else if (settings?.GroupRegistration != null)
                {
                    if (settings.GroupRegistration.MinMembers <= 0)
                    {
                        settings.GroupRegistration.MinMembers = 1;
                    }
                }
                return settings;
            }
            catch
            {
                // If deserialization fails, return default settings
                return CreateDefaultRegistrationSettings(activity.SubType);
            }
        }

        private static ActivityRegistrationSettingsDto CreateDefaultRegistrationSettings(string? subType)
        {
            // For CreativeContest, default to group registration with minMembers = 1
            if (subType == "CreativeContest")
            {
                return new ActivityRegistrationSettingsDto
                {
                    GroupRegistration = new GroupRegistrationSettingsDto
                    {
                        MinMembers = 1,
                        MaxMembers = null, // Unlimited
                        RequireLeader = true
                    }
                };
            }

            // For other activity types, default to single registration (minMembers = 1, no max limit)
            return new ActivityRegistrationSettingsDto
            {
                GroupRegistration = new GroupRegistrationSettingsDto
                {
                    MinMembers = 1,
                    MaxMembers = null, // Unlimited
                    RequireLeader = false
                }
            };
        }

        public async Task<SportRosterPaginationResponseDto> GetSportRostersAsync(SportRosterPaginationRequestDto request)
        {
            // Validate activity
            var activity = await _activityRepo.GetByIdAsync(request.ActivityId);
            if (activity == null || activity.IsDeleted)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Lấy tất cả participants có sportId và classGroupId
            var participantsQuery = _repo.GetQueryable()
                .Where(p => p.ActivityId == request.ActivityId 
                    && p.SportId.HasValue 
                    && p.ClassGroupId.HasValue);

            // Filter by sportId nếu có
            if (request.SportId.HasValue)
            {
                participantsQuery = participantsQuery.Where(p => p.SportId == request.SportId.Value);
            }

            // Include related entities
            var participants = await participantsQuery
                .Include(p => p.Sport)
                .Include(p => p.ClassGroup)
                .Include(p => p.User)
                .ToListAsync();

            // Group by SportId và ClassGroupId
            var sportRosterMap = new Dictionary<int, SportRosterDto>();

            foreach (var participant in participants)
            {
                if (!participant.SportId.HasValue || !participant.ClassGroupId.HasValue)
                    continue;

                var sportId = participant.SportId.Value;
                var classGroupId = participant.ClassGroupId.Value;

                // Tạo hoặc lấy sport roster
                if (!sportRosterMap.ContainsKey(sportId))
                {
                    sportRosterMap[sportId] = new SportRosterDto
                    {
                        SportId = sportId,
                        SportName = participant.Sport?.SportName ?? "Môn thi đấu",
                        MaxMembers = participant.Sport?.MaxMembers,
                        Classes = new List<SportRosterClassDto>()
                    };
                }

                var sportRoster = sportRosterMap[sportId];

                // Tìm hoặc tạo class roster
                var classRoster = sportRoster.Classes
                    .FirstOrDefault(c => c.ClassGroupId == classGroupId);

                if (classRoster == null)
                {
                    // Format className với grade (ví dụ: "10A1")
                    var grade = participant.ClassGroup?.Grade;
                    var className = participant.ClassGroup?.Name ?? $"Lớp {classGroupId}";
                    var formattedClassName = grade.HasValue && !string.IsNullOrEmpty(className)
                        ? $"{grade}{className}"
                        : className;

                    classRoster = new SportRosterClassDto
                    {
                        ClassGroupId = classGroupId,
                        ClassGroupName = formattedClassName,
                        Grade = grade,
                        Members = new List<SportRosterMemberDto>()
                    };
                    sportRoster.Classes.Add(classRoster);
                }

                // Thêm member vào class roster
                if (participant.User != null)
                {
                    classRoster.Members.Add(new SportRosterMemberDto
                    {
                        Id = participant.Id,
                        UserId = participant.UserId,
                        UserFullName = $"{participant.User.FirstName} {participant.User.LastName}".Trim(),
                        UserAvatarUrl = participant.User.AvatarUrl
                    });
                }
            }

            // Update member count cho mỗi class
            foreach (var sportRoster in sportRosterMap.Values)
            {
                foreach (var classRoster in sportRoster.Classes)
                {
                    classRoster.MemberCount = classRoster.Members.Count;
                }
            }

            // Convert to list và sort
            var sportRosters = sportRosterMap.Values
                .OrderBy(s => s.SportName)
                .ToList();

            // Apply search filter nếu có
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchLower = request.Search.ToLower();
                sportRosters = sportRosters
                    .Where(s => s.SportName.ToLower().Contains(searchLower)
                        || s.Classes.Any(c => c.ClassGroupName.ToLower().Contains(searchLower)
                            || c.Members.Any(m => m.UserFullName.ToLower().Contains(searchLower))))
                    .ToList();
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "sportname":
                        sportRosters = request.SortDescending
                            ? sportRosters.OrderByDescending(s => s.SportName).ToList()
                            : sportRosters.OrderBy(s => s.SportName).ToList();
                        break;
                    default:
                        // Default sort by SportName
                        sportRosters = sportRosters.OrderBy(s => s.SportName).ToList();
                        break;
                }
            }

            // Manual pagination (vì đã group rồi)
            var totalCount = sportRosters.Count;
            var pagedData = sportRosters
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new SportRosterPaginationResponseDto
            {
                Data = pagedData,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

    }
}
