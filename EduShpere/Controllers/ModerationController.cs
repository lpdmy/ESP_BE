using DocumentFormat.OpenXml.Office2010.Excel;
using EduShpere.Application.DTOs;
using EduShpere.Application;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Shared.Constants;
using EduShpere.Application.DTOs.ModerationDto;

namespace EduShpere.Controllers
{
    [ApiController]
    public class ModerationController : ControllerBase
    {
        private readonly IModerationService _moderationService;
        private readonly IHttpContextService _httpContextService;

        public ModerationController(IModerationService moderationService, IHttpContextService httpContextService)
        {
            _moderationService = moderationService;
            _httpContextService = httpContextService;
        }
        [HttpGet(ApiEndpoints.Modertaion.GetAllReports)]
        public async Task<IActionResult> GetAllModeration([FromQuery] PaginationRequestDto dto)
        {
            try
            {
                var result = await _moderationService.GetAllModeration(dto);
                return Ok(new ResponseDto<PaginationResponseDto<ModerationResponseDto>>(result, "Lấy danh sách bào cáo thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost(ApiEndpoints.Modertaion.CreateReport)]
        public async Task<IActionResult> CreateRepots([FromBody] CreateReportDTO dto)
        {
            try
            {
                var user = await _httpContextService.GetAppUserAndThrow();
                await _moderationService.CreateReportAsync(dto, user);
                return Ok(new ResponseDto<string>(null, "Tạo báo cáo thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpGet(ApiEndpoints.Modertaion.UserViolations)]
        public async Task<IActionResult> GetAllUserModeration([FromQuery] PaginationRequestDto dto)
        {
            try
            {
                var result = await _moderationService.GetAllUserModeration(dto);
                return Ok(new ResponseDto<PaginationResponseDto<UserViolationStat>>(result, "Lấy danh sách bào cáo thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost(ApiEndpoints.Modertaion.CreateAlert)]
        public async Task<IActionResult> CreateAlert([FromBody] CreateAlertDto dto)
        {
            try
            {
                 await _moderationService.SendWarningNotification(dto.UserId,dto.ContentText);
                return Ok(new ResponseDto<string>(null, "Tạo cảnh báo thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPut(ApiEndpoints.Modertaion.UpdateStatus)]
        public async Task<IActionResult> UpdateStatus(UpdateStatusDto dto)
        {
            try
            {
                await _moderationService.UpdateStatus(dto.Id, dto.Status);
                return Ok(new ResponseDto<string>(null, "Cập nhật trạng thái thành công"));
            }
            catch (BadRequestException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
