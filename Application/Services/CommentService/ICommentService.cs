using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.Services
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateComment(CreateCommentDto dto,int userid);
        Task<PaginationResponseDto<CommentResponseDto>> GetCommentByPostId(int id, int userId,
   PaginationRequestDto paginationRequest,
   string? search = null);
        Task<PaginationResponseDto<CommentResponseDto>> GetCommentByCommentId(int id, int userId,
   PaginationRequestDto paginationRequest,
   string? search = null);
        Task<CommentResponseDto> UpdateComment(int id, string content);
        Task DeleteComment(int id);
    }
}
