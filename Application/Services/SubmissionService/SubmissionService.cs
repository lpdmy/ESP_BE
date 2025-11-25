using AutoMapper;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EduShpere.Application.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ISubmissionReposiory _repo;
        private readonly IMapper _mapper;
        private readonly IActivityRepository _activityRepository;
        private readonly IJuryAssignRepository _juryAssign;
        public SubmissionService(ISubmissionReposiory repo, IMapper mapper, IActivityRepository activityRepository, IJuryAssignRepository juryAssign)
        {
            _repo = repo;
            _mapper = mapper;
            _activityRepository = activityRepository;
            _juryAssign = juryAssign;
        }

        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityId(
    int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionsByActivityId(activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            for (int i = 0; i < mapped.Count; i++)
            {
                var submission = mapped[i];
                submission.NumberJurys = await _juryAssign.GetCountAsyncBySubmission(submission.Id);
            }
            foreach (var item in mapped)
            {
                item.GradeSettings = activity.GradingSettings;
            }
            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserId(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUserByActivity(userId, activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            for (int i = 0; i < mapped.Count; i++)
            {
                var submission = mapped[i];
                submission.NumberJurys = await _juryAssign.GetCountAsyncBySubmission(submission.Id);
            }
            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserIdNotGrading(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUserByActivityNotGrading(userId, activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }

            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            for (int i = 0; i < mapped.Count; i++)
            {
                var submission = mapped[i];
                submission.NumberJurys = await _juryAssign.GetCountAsyncBySubmission(submission.Id);
            }
            foreach (var item in mapped)
            {
                item.GradeSettings = activity.GradingSettings;
            }

            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserIDGrading(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUserByActivityGrading(userId, activityId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }

            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            for (int i = 0; i < mapped.Count; i++)
            {
                var submission = mapped[i];
                submission.NumberJurys = await _juryAssign.GetCountAsyncBySubmission(submission.Id);
            }
            foreach (var item in mapped)
            {
                item.GradeSettings = activity.GradingSettings;
            }

            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<List<SubmissionResponseDto>> GetRankByActivityId(int id)
        {
            var submissions =  _repo.GetAllSubmissionsByActivityId(id);
            var requiredJuryDict = await _juryAssign.NumberJuryRequiredByActivity(id);

            var completedSubmissions = _repo.FilterCompletedSubmissions(await submissions.ToListAsync(), requiredJuryDict);

            if (!completedSubmissions.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }

            foreach (var s in completedSubmissions)
            {
                s.Score = s.JuryAssignments
                    .Where(j => j.TotalScore.HasValue)
                    .Average(j => j.TotalScore.Value);
            }
            var ranked = completedSubmissions
                .OrderByDescending(s => s.Score)
                .ToList();
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(ranked);
            return mapped;
        }
        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByUser(
        int userId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionByUser(userId);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            if (!data.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            var mapped = _mapper.Map<List<SubmissionResponseDto>>(data);
            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

        private List<string> GradeCriteria(string json)
        {
            return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
        }
    }
}
