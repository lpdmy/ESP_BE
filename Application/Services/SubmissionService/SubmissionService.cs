using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using System.Security;

namespace EduShpere.Application.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly ISubmissionReposiory _repo;
        private readonly IMapper _mapper;
        private readonly IActivityRepository _activityRepository;
        private readonly IAttachmentRepository _attachmentRepository;
        public SubmissionService(ISubmissionReposiory repo, IMapper mapper, IActivityRepository activityRepository, IAttachmentRepository attachmentRepository)
        {
            _repo = repo;
            _mapper = mapper;
            _activityRepository = activityRepository;
            _attachmentRepository = attachmentRepository;
        }

        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityId(int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllSubmissionsByActivityId(activityId);
            var activity = await _activityRepository.GetByIdWithIncludesAsync(activityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var submissions = query.ToList();
            if (!submissions.Any())
            {
                throw new BadRequestException(ErrorMessages.Submission.ListSubmissionNotFound);
            }
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Title.Contains(search));
            }
            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            
            var mapped = _mapper.Map<IEnumerable<SubmissionResponseDto>>(data);
            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

        public async Task<SubmissionResponseDto> CreateSubmission(CreateSubmissionDto dto, int userId)
        {
            // Validate activity exists
            var activity = await _activityRepository.GetByIdWithIncludesAsync(dto.ActivityId);
            if (activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }

            // Create submission
            var submission = new Submission
            {
                ActivityId = dto.ActivityId,
                UserId = userId,
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false
            };

            // Add attachments
            foreach (var attachmentDto in (dto.Attachments ?? new List<SubmissionAttachmentDto>()))
            {
                if (!string.IsNullOrWhiteSpace(attachmentDto.Url))
                {
                    submission.Attachments.Add(new Attachment
                    {
                        FileUrl = attachmentDto.Url,
                        FileName = !string.IsNullOrWhiteSpace(attachmentDto.FileName) ? attachmentDto.FileName : null,
                        FileType = attachmentDto.FileType,
                        Submission = submission,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        IsDeleted = false
                    });
                }
            }

            await _repo.AddAsync(submission);
            var submissionDto = _mapper.Map<SubmissionResponseDto>(submission);
            return submissionDto;
        }

        public async Task<SubmissionResponseDto?> GetSubmissionById(int id)
        {
            var submission = await _repo.GetByIdAsync(id);
            if (submission == null || submission.IsDeleted)
            {
                return null;
            }

            // Load attachments
            var submissionWithAttachments = await _repo.GetAllSubmissionsByActivityId(submission.ActivityId)
                .Where(s => s.Id == id && !s.IsDeleted)
                .FirstOrDefaultAsync();

            if (submissionWithAttachments == null)
            {
                return null;
            }

            return _mapper.Map<SubmissionResponseDto>(submissionWithAttachments);
        }

        public async Task<PaginationResponseDto<SubmissionResponseDto>> GetMySubmissions(int userId, PaginationRequestDto paginationRequest, string? search = null)
        {
            // Get all submissions by user from all activities
            var allSubmissions = await _repo.GetAllAsync();
            var userSubmissions = allSubmissions
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .ToList();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                userSubmissions = userSubmissions
                    .Where(s => s.Title != null && s.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalCount = userSubmissions.Count;

            // Apply pagination
            var paginatedSubmissions = userSubmissions
                .OrderByDescending(s => s.CreatedAt)
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToList();

            // Load attachments for each submission
            var submissionsWithAttachments = new List<Submission>();
            foreach (var submission in paginatedSubmissions)
            {
                var submissionWithAttachments = await _repo.GetAllSubmissionsByActivityId(submission.ActivityId)
                    .Where(s => s.Id == submission.Id)
                    .FirstOrDefaultAsync();
                if (submissionWithAttachments != null)
                {
                    submissionsWithAttachments.Add(submissionWithAttachments);
                }
            }

            var mapped = _mapper.Map<IEnumerable<SubmissionResponseDto>>(submissionsWithAttachments);
            return new PaginationResponseDto<SubmissionResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

        public async Task<SubmissionResponseDto?> GetMySubmissionByActivityId(int activityId, int userId)
        {
            var query = _repo.GetAllSubmissionsByActivityId(activityId)
                .Where(s => s.UserId == userId && !s.IsDeleted);

            var submission = await query.FirstOrDefaultAsync();
            if (submission == null)
            {
                return null;
            }

            return _mapper.Map<SubmissionResponseDto>(submission);
        }

        public async Task<SubmissionResponseDto> UpdateSubmission(UpdateSubmissionDto dto, int userId)
        {
            var submission = await _repo.GetByIdAsync(dto.Id);
            if (submission == null || submission.IsDeleted)
            {
                throw new BadRequestException("Submission không tồn tại");
            }

            // Check permission - only owner can update
            if (submission.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật submission này");
            }

            // Update title
            submission.Title = dto.Title;
            submission.UpdatedAt = DateTime.UtcNow;
            submission.UpdatedBy = userId;

            // Delete old attachments
            await _attachmentRepository.DeleteAttachmentBySubmissionId(submission.Id);

            // Add new attachments
            foreach (var attachmentDto in (dto.Attachments ?? new List<SubmissionAttachmentDto>()))
            {
                if (!string.IsNullOrWhiteSpace(attachmentDto.Url))
                {
                    submission.Attachments.Add(new Attachment
                    {
                        FileUrl = attachmentDto.Url,
                        FileName = !string.IsNullOrWhiteSpace(attachmentDto.FileName) ? attachmentDto.FileName : null,
                        FileType = attachmentDto.FileType,
                        Submission = submission,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        IsDeleted = false
                    });
                }
            }

            await _repo.UpdateAsync(submission);

            // Reload with attachments
            var updatedSubmission = await _repo.GetAllSubmissionsByActivityId(submission.ActivityId)
                .Where(s => s.Id == submission.Id)
                .FirstOrDefaultAsync();

            return _mapper.Map<SubmissionResponseDto>(updatedSubmission);
        }

        public async Task<bool> DeleteSubmission(int id, int userId)
        {
            var submission = await _repo.GetByIdAsync(id);
            if (submission == null || submission.IsDeleted)
            {
                return false;
            }

            // Check permission - only owner or admin can delete
            // Note: You may want to check if user is admin here
            if (submission.UserId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa submission này");
            }

            // Soft delete
            submission.IsDeleted = true;
            submission.UpdatedAt = DateTime.UtcNow;
            submission.UpdatedBy = userId;

            await _repo.UpdateAsync(submission);
            return true;
        }
    }
}
