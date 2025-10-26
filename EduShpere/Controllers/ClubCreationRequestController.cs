using DocumentFormat.OpenXml.Office2010.Excel;
using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    // [ApiController] // Temporarily commented out due to missing ClubCreationRequest table
    public class ClubCreationRequestController : ControllerBase
    {
        private readonly IClubCreationRequestService _service;
        private readonly IHttpContextService _httpContextService;
        public ClubCreationRequestController(IClubCreationRequestService service, IHttpContextService httpContextService)
        {
            _service = service;
            _httpContextService = httpContextService;
        }
        [HttpPost(ApiEndpoints.ClubCreationRequest.Create)]
        public async Task<IActionResult> CreateClubRequest([FromBody] CreateClubRequestDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
           
            try
            {
                var result = await _service.createClubRequest(dto, user);
                return Ok(new ResponseDto<ClubCreationResponseDto>(result, "tạo đơn tạo club thành công ", 200));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet(ApiEndpoints.ClubCreationRequest.GetAll)]
        public async Task<IActionResult> GetAllClubCreationRequests([FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null, int ? status =null)
        {
            var response = await _service.GetAllAsync(paginationRequest, search, status);
            return Ok(new ResponseDto<PaginationResponseDto<ClubCreationResponseDto>>(
                       response,
                       message: "Lấy bộ sưu tập thành công",
                       statusCode: 200
                   ));
        }
        [HttpGet(ApiEndpoints.ClubCreationRequest.GetAllByUser)]
        public async Task<IActionResult> GetAllClubCreationRequestsByUser([FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var response = await _service.GetAllAsyncByUser(user, paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<ClubCreationResponseDto>>(
                       response,
                       message: "Lấy bộ sưu tập thành công",
                       statusCode: 200
                   ));
        }
        [HttpPut(ApiEndpoints.ClubCreationRequest.Approve)]
        public async Task<IActionResult> ApproveClubCreationRequest(int id)
        {
            try
            {
                var result = await _service.ApproveCreation(id);
                return Ok(new ResponseDto<ClubCreationResponseDto>(result, "Duyệt đơn tạo câu lạc bộ thành công", 200));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });

            }
        }
        [HttpPut(ApiEndpoints.ClubCreationRequest.Reject)]
        public async Task<IActionResult> RejectClubCreationRequest([FromBody]RejectCreationDto dto)
        {
            try
            {
                var result = await _service.RejectCreation(dto);
                return Ok(new ResponseDto<ClubCreationResponseDto>(result, "Duyệt đơn tạo câu lạc bộ thành công", 200));
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });

            }
        }

    }
}
