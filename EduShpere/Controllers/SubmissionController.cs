using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SubmissionDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
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
    }
}
