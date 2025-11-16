using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EduShpere.Application.Services
{
    public class JuryService : IJuryService
    {
        private readonly IJuryActivityRepository _juryActivityRepo;
        private readonly IJuryAssignRepository _juryAssignRepo;
        private readonly IMapper _mapper;
        private readonly IActivityRepository _activityRepo;
        private readonly INotificationService _notificationService;
        private readonly ISubmissionReposiory _submissionRepository;
        public JuryService(IJuryActivityRepository juryActivityRepo, IMapper mapper, IActivityRepository activityRepository, IJuryAssignRepository juryAssignRepo, INotificationService notificationService, ISubmissionReposiory submissionRepository)
        {
            _juryActivityRepo = juryActivityRepo;
            _mapper = mapper;
            _activityRepo = activityRepository;
            _juryAssignRepo = juryAssignRepo;
            _notificationService = notificationService;
            _submissionRepository = submissionRepository;
        }

        public async Task<List<JuryActivityResponseDto>> GetAllJuryByActivityIdAsync(int activityId, string? search)
        {
            var query = _juryActivityRepo.GetAllJuryActivityByActivityIdIncluding(activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x =>
                    x.User.FirstName.Contains(search) ||
                    x.User.LastName.Contains(search));
            }

            var result = await query
                .Select(x => new JuryActivityResponseDto
                {
                    UserId = x.UserId,
                    ActivityId = x.ActivityId,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Assigned = _juryAssignRepo.GetCountAsyncByUser(x.UserId)
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<JuryActivityResponseDto>> CreateJury(CreateJuryDto dto)
        {
            var activity = await _activityRepo.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var result = new List<JuryActivityResponseDto>();
            foreach (var userId in dto.JuryId)
            {
                var isExisting = await _juryActivityRepo.IsExisting(userId, dto.ActivityId);
                if (!isExisting)
                {
                    var juryActivity = new JuryActivity
                    {
                        UserId = userId,
                        ActivityId = dto.ActivityId
                    };
                    await _juryActivityRepo.AddAsync(juryActivity);
                    result.Add(new JuryActivityResponseDto
                    {
                        UserId = juryActivity.UserId,
                        ActivityId = juryActivity.ActivityId,
                    });
                    await _notificationService.AddAsync(new Notification
                    {
                        Link= "/jury/dashboard",
                        Title = $" Bạn đã được thêm vào hội đồng chấm thi cuộc thi {activity.Title}",
                        CreatedAt = DateTime.Now,
                        Read = false,
                        Type = "system",
                        UserId = userId,
                    });
                }
            }
            return result;
        }
        public async Task DeletedJury(int id)
        {
            var juryActivity = await _juryActivityRepo.GetByIdAsync(id);
            if (juryActivity == null)
            {
                throw new BadRequestException(ErrorMessages.Jury.JuryNotFound);
            }
            await _juryActivityRepo.DeleteAsync(juryActivity.Id);
        }
        public async Task<List<JuryAssignmentDto>> AssignJury(AssignJuryDto dto)
        {
            var result = new List<JuryAssignmentDto>();
            var oldAssignments = await _juryAssignRepo.GetAllBySubmissionId(dto.SubmissionId);
            foreach (var old in oldAssignments)
            {
                await _juryAssignRepo.DeleteAsync(old.Id);
            }
            foreach (var userId in dto.UserId)
            {

                var assignJury = new JuryAssignment
                {
                    UserId = userId,
                    SubmissionId = dto.SubmissionId
                };

                await _juryAssignRepo.AddAsync(assignJury);
                result.Add(new JuryAssignmentDto
                {
                    UserId = assignJury.UserId,
                    SubmissionId = dto.SubmissionId,
                });

            }

            return result;
        }
        public async Task<PaginationResponseDto<JuryActivityResponseDto>> GetAllByUserAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _juryActivityRepo.GetAllJuryActivityByUser(user.Id);

            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.Activity.Title.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            var list = query.ToList();
            if (!list.Any())
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<IEnumerable<JuryActivityResponseDto>>(data);
            var sql = query.ToQueryString();
            return new PaginationResponseDto<JuryActivityResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<bool> AutoAssignRandomAsync(int activityId, int juryPerSubmission)
        {
            var submissions = _submissionRepository.GetAllSubmissionsByActivityId(activityId);
            var juryActivities = _juryActivityRepo.GetAllJuryActivityByActivityIdIncluding(activityId);
            var juryUserIds = juryActivities.Select(j => j.UserId).ToList();
            if (juryUserIds.Count < juryPerSubmission)
                throw new BadRequestException("Số lượng giám khảo không đủ.");
            var assignments = await _juryAssignRepo.GetAllByActivityId(activityId);
            var random = new Random();
            var newAssignments = new List<JuryAssignment>();
            foreach (var submission in submissions)
            {
                var assignedIds = assignments
                    .Where(a => a.SubmissionId == submission.Id)
                    .Select(a => a.UserId)
                    .ToList();
                int need = juryPerSubmission - assignedIds.Count;
                if (need <= 0) continue;
                var available = juryUserIds
                    .Where(uid => !assignedIds.Contains(uid))
                    .ToList();
                var selected = available
                    .OrderBy(_ => random.Next())
                    .Take(need)
                    .ToList();
                foreach (var uid in selected)
                {
                    newAssignments.Add(new JuryAssignment
                    {
                        SubmissionId = submission.Id,
                        UserId = uid
                    });
                }
            }
            if (newAssignments.Count > 0)
            {
                await _juryAssignRepo.AddRangeAsync(newAssignments);
            }
            return true;
        }
        public async Task<bool> DeleteAssignJury(int activityId)
        {
            var activity = await _activityRepo.GetByIdAsync(activityId);
            if (activityId <= 0 || activity==null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            await _juryAssignRepo.DeleteAllByActivityIdAsync(activityId);
            return true;
        }
        public async Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserAsync(int userid,int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _juryAssignRepo.GetAllByActivityIdUserId(activityId, userid);
            var totalCount = await query.CountAsync();
            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.Submission.Title.Contains(paginationRequest.Search));
            }
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            var list = query.ToList();
            if (!list.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            var mapped = _mapper.Map<IEnumerable<JuryAssignmentDto>>(data);
            var sql = query.ToQueryString();
            return new PaginationResponseDto<JuryAssignmentDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserNotGradeAsync(int userid, int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _juryAssignRepo.GetAllByActivityIdUserIdNotGrading(activityId, userid);
            var totalCount = await query.CountAsync();
            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.Submission.Title.Contains(paginationRequest.Search));
            }
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            var list = query.ToList();
            if (!list.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            var mapped = _mapper.Map<IEnumerable<JuryAssignmentDto>>(data);
            var sql = query.ToQueryString();
            return new PaginationResponseDto<JuryAssignmentDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
    }
}
