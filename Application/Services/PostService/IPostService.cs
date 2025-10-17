
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.Services
{
    public interface IPostService
    {
        Task<PostResponseDto> CreatePost(CreatePostDto dto, User user);
        Task<IEnumerable<PostResponseDto>> GetAllPostsGeneral(User currentUser);
        Task<IEnumerable<PostResponseDto>> GetPostByUserId(User user, string sortOrder = "newst");
        Task<PostResponseDto> DeletePost(int id);
        Task<PostResponseDto> UpdatePost(UpdatePostDto dto);
        Task<PostResponseDto?> PostLike(CreatePostLikeDto dto, User user);
        Task<PaginationResponseDto<PostResponseDto>> GetAllPostsClubPending(int clubid,
   PaginationRequestDto paginationRequest,
   string? search = null);
        Task<IEnumerable<PostResponseDto>> GetAllPostsClub(int clubid, User user);
        Task<PostResponseDto> ApprovePost(int id);
    }
}
