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
    [ApiController]
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
        public async Task<IActionResult> GetAllClubCreationRequests([FromQuery] PaginationRequestDto paginationRequest, [FromQuery] string? search = null)
        {
            var response = await _service.GetAllAsync(paginationRequest, search);
            return Ok(new ResponseDto<PaginationResponseDto<ClubCreationResponseDto>>(
                       response,
                       message: "Lấy bộ sưu tập thành công",
                       statusCode: 200
                   ));
        }
    }
}
