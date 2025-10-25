using EduShpere.Application.DTOs;
using EduShpere.Application;
using EduShpere.Application.DTOs.PostDto;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Shared;
using Microsoft.Data.SqlClient;
using EduShpere.Application.DTOs.CommonDto;

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

        [HttpGet(ApiEndpoints.Post.GetPostsByClassGroup)]
        public async Task<IActionResult> GetPostsByClassGroup(int id)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _postService.GetPostsByClassGroup(id, user);
                return Ok(new ResponseDto<IEnumerable<PostResponseDto>>(result, "Lấy danh sách bài viết của lớp học thành công", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi khi lấy danh sách bài viết của lớp học",
                    error = ex.Message
                });
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
        [HttpGet(ApiEndpoints.Post.ClubPending)]
        public async Task<IActionResult> GetAllPostByClubPending(int clubid, [FromQuery]PaginationRequestDto paginationRequestDto)
        {
            try
            {
                var result = await _postService.GetAllPostsClubPending(clubid, paginationRequestDto);
                return Ok(new ResponseDto<PaginationResponseDto<PostResponseDto>>(result, "Lấy danh sách bài viết của câu lạc bộ thành công", 200));
            }
            catch(BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            
        }
        [HttpGet(ApiEndpoints.Post.Club)]
        public async Task<IActionResult> GetAllPostByClub(int clubid)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _postService.GetAllPostsClub(clubid, user);
                return Ok(new ResponseDto<IEnumerable<PostResponseDto>>(result, "Lấy danh sách bài viết của câu lạc bộ thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
        }
        [HttpPut(ApiEndpoints.Post.ApprovePost)]
        public async Task<IActionResult> ApprovePost(int id)
        {
            try
            {
                var result = await _postService.ApprovePost(id);
                return Ok(new ResponseDto<PostResponseDto>(result, "Duyệt bài viết thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
        }
        [HttpPut(ApiEndpoints.Post.RejectPost)]
        public async Task<IActionResult> RejectPost(int id)
        {
            try
            {
                var result = await _postService.RejectPost(id);
                return Ok(new ResponseDto<PostResponseDto>(result, "Từ chối duyệt bài viết thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
        }
    }
}
