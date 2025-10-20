using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IHttpContextService _httpContextService;
        public CommentController(ICommentService commentService, IHttpContextService httpContextService)
        {
            _commentService = commentService;
            _httpContextService = httpContextService;
        }

        [HttpPost(ApiEndpoints.Comment.Comments)]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _commentService.CreateComment(dto, user.Id);
                return Ok(new ResponseDto<CommentResponseDto>(result, "Tạo bình luận thành công", 200));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet(ApiEndpoints.Comment.GetCommentsByPost)]
        public async Task<IActionResult> GetCommentsByPost(int postId, [FromQuery] PaginationRequestDto paginationRequest)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try {
                var response = await _commentService.GetCommentByPostId(postId, user.Id, paginationRequest);
                return Ok(new ResponseDto<PaginationResponseDto<CommentResponseDto>>(
                            response,
                            message: "Lấy danh sách câu lạc bộ thành công",
                            statusCode: 200
                        ));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
            
        }
        [HttpGet(ApiEndpoints.Comment.GetCommentsByComment)]
        public async Task<IActionResult> GetCommentsByComment(int id, [FromQuery] PaginationRequestDto paginationRequest)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _commentService.GetCommentByCommentId(id, user.Id, paginationRequest);
            return Ok(new ResponseDto<PaginationResponseDto<CommentResponseDto>>(
                        response,
                        message: "Lấy danh sách câu lạc bộ thành công",
                        statusCode: 200
                    ));
        }
        [HttpPut(ApiEndpoints.Comment.Comments)]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentDto dto)
        {
            try
            {
                var response = await _commentService.UpdateComment(dto.CommentId, dto.Content);
                return Ok(new ResponseDto<CommentResponseDto>(
                           response,
                           message: "Cập nhật bình luận thành công",
                           statusCode: 200
                       ));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
        }
        [HttpDelete(ApiEndpoints.Comment.DeleteComment)]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                await _commentService.DeleteComment(id);
                return Ok(new ResponseDto<CommentResponseDto>(
                           null,
                           message: "Xóa bình luận thành công",
                           statusCode: 200
                       ));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
        }

    }
}
