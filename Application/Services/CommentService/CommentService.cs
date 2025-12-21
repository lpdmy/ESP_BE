using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using EduShpere.Application.Services.StarPointService;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IMapper _mapper;
        private readonly ContentModerationService _contentModerationService;
        private readonly IModerationService _moderationService;
        private readonly IUserActionRewardService _userActionRewardService;
        public CommentService(ICommentRepository commentRepository, IMapper mapper, ContentModerationService contentModerationService, IModerationService moderationService, IUserActionRewardService userActionRewardService)
        {
            _commentRepository = commentRepository;
            _mapper = mapper;
            _contentModerationService = contentModerationService;
            _moderationService = moderationService;
            _userActionRewardService = userActionRewardService;
        }

        public async Task<CommentResponseDto> CreateComment(CreateCommentDto dto,int userId)
        {
            var comment = new Comment
            {
                Content = dto.Content,
                PostId = dto.PostId,
                UserId = userId,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                ParentCommentId = dto.ParentCommentId
            };
            var moderationResult = _contentModerationService.Check(comment.Content);
            if (moderationResult.Decision == "block")
            {
                await _moderationService.CreateReport(new DTOs.ModerationDto.AddModerationDto
                {
                    ContentText = comment.Content,
                    AuthorId = comment.UserId,
                    ReporterId = 0,
                    Status = "Approved"
                });
                throw new BadRequestException(ErrorMessages.Moderation.ContentViolation);
            }
            await _commentRepository.AddAsync(comment);

            // Cộng điểm cho hành động bình luận bài viết
            await _userActionRewardService.AwardForActionAsync(userId, RewardActionType.CommentPost);

            return _mapper.Map<CommentResponseDto>(comment);
        }
        public async Task<PaginationResponseDto<CommentResponseDto>> GetCommentByPostId(int id,int userId,
   PaginationRequestDto paginationRequest,
   string? search = null)
        {
            var comments =  _commentRepository.GetCommentsByPostIdAsync(id);
            if (comments == null)
            {
                throw new BadRequestException(ErrorMessages.Comment.CommentNotFound);
            }

            var totalCount = await comments.CountAsync();

            var data = await comments
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            var mapped = _mapper.Map<List<CommentResponseDto>>(data);
            foreach (var item in mapped)
            {
                item.IsCurrentUser = item.UserId == userId;
            }
            return new PaginationResponseDto<CommentResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<CommentResponseDto>> GetCommentByCommentId(int id, int userId,
   PaginationRequestDto paginationRequest,
   string? search = null)
        {
            var comments = _commentRepository.GetCommentsByCommentIdAsync(id);
            if (comments == null)
            {
                throw new BadRequestException(ErrorMessages.Comment.CommentNotFound);
            }

            var totalCount = await comments.CountAsync();
             
            var data = await comments
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            var mapped = _mapper.Map<List<CommentResponseDto>>(data);
            foreach (var item in mapped) {
                item.IsCurrentUser = item.UserId == userId;
            }
            return new PaginationResponseDto<CommentResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<CommentResponseDto> UpdateComment(int id, string content)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                throw new BadRequestException(ErrorMessages.Comment.CommentNotFound);
            }
            comment.Content = content;
            comment.UpdatedAt = DateTime.Now;
            await _commentRepository.UpdateAsync(comment);
            return _mapper.Map<CommentResponseDto>(comment);
        }
        public async Task DeleteComment(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                throw new BadRequestException(ErrorMessages.Comment.CommentNotFound);
            }
            await _commentRepository.DeleteAsync(id);
        }
    }
}
