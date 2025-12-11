using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.ModerationDto;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using static EduShpere.Shared.Constants.ApiEndpoints;

namespace EduShpere.Application.Services
{
    public class ModerationService : IModerationService
    {
        private readonly IModerationRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICommentRepository _commentService;
        private readonly INotificationService _notificationService; 
        public ModerationService(IModerationRepository repository, IMapper mapper, ICommentRepository commentService, INotificationService notificationService)
        {
            _repository = repository;
            _mapper = mapper;
            _commentService = commentService;
            _notificationService = notificationService;
        }
        public async Task CreateReport(AddModerationDto dto)
        {
            var report = new ReportedContent
            {
                ContentText = dto.ContentText,
                AuthorId = dto.AuthorId,
                ReporterId = dto.ReporterId,
                Status = dto.Status,
            };
            await _repository.AddAsync(report);
        }

        public async Task CreateReportAsync(CreateReportDTO dto, Domain.Models.User user)
        {
            if (user.Id == dto.AuthorId)
            {
                throw new BadRequestException("Bạn không thể báo cáo bình luận của bản thân mình");
            }
            var report = new ReportedContent
            {
                ContentText = dto.ContentText,
                AuthorId = dto.AuthorId,
                Status = dto.Status,
                ReporterId = user.Id,
            };
            await _repository.AddAsync(report);
        }
        public async Task<PaginationResponseDto<ModerationResponseDto>> GetAllModeration(
   PaginationRequestDto paginationRequest,
   string? search = null)
        {
            var moderations = _repository.GetAllModerationIncluded();
            if (moderations == null)
            {
                throw new BadRequestException(ErrorMessages.Moderation.ListNotFound);
            }

            var totalCount = await moderations.CountAsync();
            var data = await moderations
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            var mapped = _mapper.Map<List<ModerationResponseDto>>(data);
            return new PaginationResponseDto<ModerationResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<UserViolationStat>> GetAllUserModeration(
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repository.GetUserViolationStatsRaw();

            if (query == null)
            {
                throw new BadRequestException(ErrorMessages.Moderation.ListNotFound);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.AuthorName.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var result = items.Select(x => new UserViolationStat
            {
                AuthorId = x.AuthorId,
                AuthorName = x.AuthorName,
                ViolationCount = x.ViolationCount,
                LatestViolation = x.LatestViolation
            }).ToList();

            return new PaginationResponseDto<UserViolationStat>
            {
                Data = result,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task SendWarningNotification(int UserId,string message)
        {
            await _notificationService.AddAsync(new Domain.Models.Notification
            {
                Title = $"Nhà trường đã gửi cho bạn thông báo \"{message}\"",
                CreatedAt = DateTime.Now,
                Read = false,
                Type = "system",
                UserId = UserId
            });
        }
        public async Task UpdateStatus(int id,int status)
        {
            var report = await _repository.GetByIdAsync(id);
            if (report == null)
            {
                throw new BadRequestException(ErrorMessages.Moderation.NotFound);
            }
            if(status == 1) { 
            report.Status = "Approved";
            }
            else
            {
                report.Status = "Rejected";
            }
            await _repository.UpdateAsync(report);
        }

    }
}
