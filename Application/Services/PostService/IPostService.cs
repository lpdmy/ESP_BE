
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IPostService
    {
        Task<PostResponseDto> CreatePost(CreatePostDto dto, User user);
        Task<IEnumerable<PostResponseDto>> GetAllPostsGeneral(User currentUser);
        Task<IEnumerable<PostResponseDto>> GetPostByUserId(User user, string sortOrder = "newst");
        Task<IEnumerable<PostResponseDto>> GetPostsByClassGroup(int classGroupId, User currentUser);
        Task<PostResponseDto> DeletePost(int id);
        Task<PostResponseDto> UpdatePost(UpdatePostDto dto);
        Task<PostResponseDto?> PostLike(CreatePostLikeDto dto, User user);
    }
}
