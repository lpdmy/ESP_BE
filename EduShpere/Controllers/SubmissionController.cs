using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAllByActivityId([FromQuery] PaginationRequestDto dto, int Id, string? search)
        {
            try
            {
                var result = await _service.GetAllSubmissionByActivityId(Id, dto, search);
                return Ok(new ResponseDto<PaginationResponseDto<SubmissionResponseDto>>(result, "Lấy danh sách thành công", 200));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        [HttpGet(ApiEndpoints.Submission.GetAllByUserIdByActivityId)]
        public async Task<IActionResult> GetAllByActivityIdByUserId([FromQuery] PaginationRequestDto dto, int Id, string? search)
        {
            var userId = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _service.GetAllSubmissionByActivityIdByUserId(Id, userId.Id, dto, search);
                return Ok(new ResponseDto<PaginationResponseDto<SubmissionResponseDto>>(result, "Lấy danh sách thành công", 200));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpGet(ApiEndpoints.Submission.GetRankByActivityId)]
        public async Task<IActionResult> GetRankByActivityId(int id)
        {
            try
            {
                var result = await _service.GetRankByActivityId(id);
                return Ok(new ResponseDto<List<SubmissionResponseDto>>(result, "Lấy bảng xếp hạng thành công", 200));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpGet(ApiEndpoints.Submission.GetAllByUser)]
        public async Task<IActionResult> GetSubmissionByUser([FromQuery] PaginationRequestDto dto)
        {
            var userId = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _service.GetAllSubmissionByUser(userId.Id, dto);
                return Ok(new ResponseDto<PaginationResponseDto<SubmissionResponseDto>>(result, "Lấy danh sách bài nộp thành công", 200));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpGet(ApiEndpoints.Submission.GetSubmissionById)]
        public async Task<IActionResult> GetSubmissionById(int id)
        {
            try
            {
                var result = await _service.GetSubmissionById(id);
                return Ok(new ResponseDto<SubmissionResponseDto>(result, "Lấy bài nộp thành công", 200));
            }
            catch (BadRequestException err)
            {
                throw new BadRequestException(err.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet("api/activities/{activityId}/submission-status")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetSubmissionStatus(int activityId)
        {
            try
            {
                var userId = await _httpContextService.GetAppUserAndThrow();
                var result = await _service.GetSubmissionStatusAsync(activityId, userId.Id);
                return Ok(new ResponseDto<SubmissionStatusDto>(result, "Lấy trạng thái nộp bài thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<SubmissionStatusDto>(null, err.Message, 400));
            }
            catch (NotFoundException err)
            {
                return NotFound(new ResponseDto<SubmissionStatusDto>(null, err.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<SubmissionStatusDto>(null, ex.Message, 500));
            }
        }

        [HttpPost("api/activities/{activityId}/submissions")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> CreateSubmission(int activityId, [FromBody] CreateSubmissionDto dto)
        {
            try
            {
                var userId = await _httpContextService.GetAppUserAndThrow();
                dto.ActivityId = activityId; // Ensure activityId matches route
                var result = await _service.CreateSubmissionAsync(dto, userId.Id);
                return Ok(new ResponseDto<SubmissionResponseDto>(result, "Nộp bài thành công", 200));
            }
            catch (BadRequestException err)
            {
                return BadRequest(new ResponseDto<SubmissionResponseDto>(null, err.Message, 400));
            }
            catch (NotFoundException err)
            {
                return NotFound(new ResponseDto<SubmissionResponseDto>(null, err.Message, 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<SubmissionResponseDto>(null, ex.Message, 500));
            }
        }

        [HttpGet("api/activities/{activityId}/submissions/my")]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetMySubmission(int activityId)
        {
            try
            {
                var userId = await _httpContextService.GetAppUserAndThrow();
                var result = await _service.GetMySubmissionAsync(activityId, userId.Id);
                if (result == null)
                {
                    return NotFound(new ResponseDto<SubmissionResponseDto>(null, "Chưa có bài nộp", 404));
                }
                return Ok(new ResponseDto<SubmissionResponseDto>(result, "Lấy bài nộp thành công", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<SubmissionResponseDto>(null, ex.Message, 500));
            }
        }
    }
}
