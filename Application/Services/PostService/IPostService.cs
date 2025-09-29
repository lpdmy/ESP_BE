
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IPostService
    {
        Task<PostResponseDto> CreatePost(CreatePostDto dto, User user);
        Task<IEnumerable<PostResponseDto>> getAllPostsGeneral();
        Task<IEnumerable<PostResponseDto>> GetPostByUserId(User user, string sortOrder = "desc");
        Task<PostResponseDto> DeletePost(int id);
        Task<PostResponseDto> UpdatePost(UpdatePostDto dto);
    }
}
