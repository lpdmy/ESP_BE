using EduShpere.Application.DTOs;
using EduShpere.Application;
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Shared;
using Microsoft.Data.SqlClient;

namespace EduShpere.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IHttpContextService _httpContextService;
        private readonly ILogger<PostController> _logger;
        public PostController(IPostService postService, IHttpContextService httpContextService, ILogger<PostController> logger)
        {
            _postService = postService;
            _httpContextService = httpContextService;
            _logger = logger;
        }
        [HttpPost(ApiEndpoints.Post.Posts)]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _postService.CreatePost(dto,user);
                return Ok(new ResponseDto<PostResponseDto>(result, "Tạo bài viết thành công", 200));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ErrorMessages.Post.PostError,
                    error = ex.Message,
                    inner = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }

        }
        [HttpGet(ApiEndpoints.Post.Posts)]
        public async Task<IActionResult> getAllPost()
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var result = await _postService.GetAllPostsGeneral(user);
            return Ok(new ResponseDto<IEnumerable<PostResponseDto>>(result, "Lấy danh sách bài viết thành công", 200));
        }

        [HttpGet(ApiEndpoints.Post.GetPostByUser)]
        public async Task<IActionResult> getPostByUser([FromQuery] string sortOrder)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try {
                var result = await _postService.GetPostByUserId(user, sortOrder);
                return Ok(new ResponseDto<IEnumerable<PostResponseDto>>(result, "Lấy danh sách bài viết của người dùng thành công", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }

        }
        [HttpDelete(ApiEndpoints.Post.Posts)]
        public async Task<IActionResult> deletePost(int id)
        {
            var result = await _postService.DeletePost(id);
            return Ok(new ResponseDto<PostResponseDto>(result, "Xóa bài viết thành công", 200));
        }
        [HttpPut(ApiEndpoints.Post.Posts)]
        public async Task<IActionResult> UpdatePost(UpdatePostDto dto)
        {
            var result = await _postService.UpdatePost(dto);
            return Ok(new ResponseDto<PostResponseDto>(result, "Chỉnh sửa bài viết thành công", 200));
        }
        [HttpPost(ApiEndpoints.Post.Like)]
        public async Task<IActionResult> LikePost(CreatePostLikeDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _postService.PostLike(dto, user);
                return Ok(new ResponseDto<PostResponseDto>(result, "Thích bài viết thành công", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
