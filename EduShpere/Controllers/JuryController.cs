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
    public class JuryController : ControllerBase
    {
        private readonly IJuryService _juryService;
        private readonly IHttpContextService _httpContextService;
        public JuryController(IJuryService juryService, IHttpContextService httpContextService)
        {
            _juryService = juryService;
            _httpContextService = httpContextService;
        }
        [HttpGet(ApiEndpoints.Jury.GetAllByClubId)]
        public async Task<IActionResult> GetAllJuryByActivityId(int id, [FromQuery] string? Search)
        {
            try
            {
                var result = await _juryService.GetAllJuryByActivityIdAsync(id, Search);
                return Ok(new ResponseDto<List<JuryActivityResponseDto>>(result, "Lấy danh sách thành công"));
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
        [HttpPost(ApiEndpoints.Jury.ApiJury)]
        public async Task<IActionResult> CreateJury([FromBody] CreateJuryDto dto)
        {
            try
            {
                var result = await _juryService.CreateJury(dto);
                return Ok(new ResponseDto<List<JuryActivityResponseDto>>(result, "Tạo giám khảo thành công"));
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
        [HttpDelete(ApiEndpoints.Jury.GetAllByClubId)]
        public async Task<IActionResult> DeleteJury(int id)
        {
            try
            {
                await _juryService.DeletedJury(id);
                return Ok(new ResponseDto<string>(null, "Xóa giám khảo thành công"));
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
        [HttpPost(ApiEndpoints.Jury.assignJury)]
        public async Task<IActionResult> AssignJury([FromBody] AssignJuryDto dto)
        {
            try
            {
                var result = await _juryService.AssignJury(dto);
                return Ok(new ResponseDto<List<JuryAssignmentDto>>(result, "Phân công giám khảo thành công"));
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
        [HttpGet(ApiEndpoints.Jury.GetJuryActivity)]
        public async Task<IActionResult> GetAllJuryActivity([FromQuery] PaginationRequestDto paginationRequest)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _juryService.GetAllByUserAsync(user, paginationRequest);
                return Ok(new ResponseDto<PaginationResponseDto<JuryActivityResponseDto>>(result, "Lấy danh sách sự kiện thành công"));
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
        [HttpPost(ApiEndpoints.Jury.RamdomAssignJury)]
        public async Task<IActionResult> RandomAssignJury([FromBody] RamdomAssignJuryDto dto)
        {
            try
            {
                var result = await _juryService.AutoAssignRandomAsync(dto.ActivityId, dto.NumberOfJury);
                return Ok(new ResponseDto<bool>(result, "Phân công giám khảo ngẫu nhiên thành công"));
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
        [HttpDelete(ApiEndpoints.Jury.DeleteAssignJury)]
        public async Task<IActionResult> DeleteAssignJury(int id)
        {
            try
            {
                await _juryService.DeleteAssignJury(id);
                return Ok(new ResponseDto<string>(null, "Xóa phân công giám khảo thành công"));
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
        [HttpGet(ApiEndpoints.Jury.GetAllAssignByUser)]
        public async Task<IActionResult> GetAllAssignByUserAsync( int Id, [FromQuery] PaginationRequestDto paginationRequest)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _juryService.GetAllAssignByUserAsync(user.Id, Id, paginationRequest);
                return Ok(new ResponseDto<PaginationResponseDto<JuryAssignmentDto>>(result, "Lấy danh sách phân công giám khảo thành công"));
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
        [HttpGet(ApiEndpoints.Jury.GetAllAssignByUserNotGrading)]
        public async Task<IActionResult> GetAllAssignByUserNotGradingAsync(int Id, [FromQuery] PaginationRequestDto paginationRequest)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _juryService.GetAllAssignByUserNotGradeAsync(user.Id, Id, paginationRequest);
                return Ok(new ResponseDto<PaginationResponseDto<JuryAssignmentDto>>(result, "Lấy danh sách phân công giám khảo thành công"));
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
