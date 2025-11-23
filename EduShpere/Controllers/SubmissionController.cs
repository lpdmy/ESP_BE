using EduShpere.Application;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduShpere.Controllers
{
    [ApiController]
    public class SubmissionController : ControllerBase
    {
        private readonly ISubmissionService _service;
        private readonly IHttpContextService _httpContextService;

        public SubmissionController(ISubmissionService service, IHttpContextService httpContextService)
        {
            _service = service;
            _httpContextService = httpContextService;
        }

        [HttpGet(ApiEndpoints.Submission.GetAllByActivityId)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetAllByActivityId([FromQuery] PaginationRequestDto dto, int Id, string? search)
        {
            try
            {
                var result = await _service.GetAllSubmissionByActivityId(Id, dto, search);
                return Ok(new ResponseDto<PaginationResponseDto<SubmissionResponseDto>>(result, "Lấy danh sách thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, ex.Message, 500));
            }
        }

        [HttpPost(ApiEndpoints.Submission.Create)]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateSubmission([FromBody] CreateSubmissionDto dto)
        {
            try
            {
                var userId = _httpContextService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));
                }

                var result = await _service.CreateSubmission(dto, userId.Value);
                return Ok(new ResponseDto<SubmissionResponseDto>(result, "Tạo submission thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, ex.Message, 500));
            }
        }

        [HttpGet(ApiEndpoints.Submission.GetById)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetSubmissionById(int id)
        {
            try
            {
                var result = await _service.GetSubmissionById(id);
                if (result == null)
                {
                    return NotFound(new ResponseDto<string>(null, "Submission không tồn tại", 404));
                }
                return Ok(new ResponseDto<SubmissionResponseDto>(result, "Lấy submission thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, ex.Message, 500));
            }
        }

        [HttpGet(ApiEndpoints.Submission.GetMySubmissions)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMySubmissions([FromQuery] PaginationRequestDto dto, string? search = null)
        {
            try
            {
                var userId = _httpContextService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));
                }

                var result = await _service.GetMySubmissions(userId.Value, dto, search);
                return Ok(new ResponseDto<PaginationResponseDto<SubmissionResponseDto>>(result, "Lấy danh sách submission của bạn thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, ex.Message, 500));
            }
        }

        [HttpGet(ApiEndpoints.Submission.GetMySubmissionByActivityId)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMySubmissionByActivityId(int activityId)
        {
            try
            {
                var userId = _httpContextService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));
                }

                var result = await _service.GetMySubmissionByActivityId(activityId, userId.Value);
                if (result == null)
                {
                    return NotFound(new ResponseDto<string>(null, "Bạn chưa nộp bài cho activity này", 404));
                }
                return Ok(new ResponseDto<SubmissionResponseDto>(result, "Lấy submission thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, ex.Message, 500));
            }
        }

        [HttpPut(ApiEndpoints.Submission.Update)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> UpdateSubmission(int id, [FromBody] UpdateSubmissionDto dto)
        {
            try
            {
                if (dto.Id != id)
                {
                    return BadRequest(new ResponseDto<string>(null, "Id không khớp", 400));
                }

                var userId = _httpContextService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));
                }

                var result = await _service.UpdateSubmission(dto, userId.Value);
                return Ok(new ResponseDto<SubmissionResponseDto>(result, "Cập nhật submission thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ResponseDto<string>(null, "Bạn không có quyền cập nhật submission này", 401));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, ex.Message, 500));
            }
        }

        [HttpDelete(ApiEndpoints.Submission.Delete)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> DeleteSubmission(int id)
        {
            try
            {
                var userId = _httpContextService.GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));
                }

                var result = await _service.DeleteSubmission(id, userId.Value);
                if (!result)
                {
                    return NotFound(new ResponseDto<string>(null, "Submission không tồn tại", 404));
                }
                return Ok(new ResponseDto<bool>(true, "Xóa submission thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<string>(null, err.Message, 400));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ResponseDto<string>(null, "Bạn không có quyền xóa submission này", 401));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<string>(null, ex.Message, 500));
            }
        }
    }
}
