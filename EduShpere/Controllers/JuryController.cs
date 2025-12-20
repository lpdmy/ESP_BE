using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Admin,Staff")]
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
        [Authorize(Roles = "Admin,Staff")]
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
        public async Task<IActionResult> GetAllAssignByUserAsync(int Id, [FromQuery] PaginationRequestDto paginationRequest)
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
        [HttpGet(ApiEndpoints.Jury.GetAllAssignByUserGrading)]
        public async Task<IActionResult> GetAllAssignByUserGradingAsync(int Id, [FromQuery] PaginationRequestDto paginationRequest)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _juryService.GetAllAssignByUserGradeAsync(user.Id, Id, paginationRequest);
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
        
        /// <summary>
        /// Lấy toàn bộ danh sách assignments chưa chấm của user (không pagination)
        /// </summary>
        [HttpGet(ApiEndpoints.Jury.GetAllAssignByUserNotGradingAll)]
        public async Task<IActionResult> GetAllAssignByUserNotGradingAllAsync(int Id, [FromQuery] string? search = null)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _juryService.GetAllAssignByUserNotGradeAllAsync(user.Id, Id, search);
                return Ok(new ResponseDto<List<JuryAssignmentDto>>(result, "Lấy danh sách phân công giám khảo thành công"));
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
        
        /// <summary>
        /// Lấy toàn bộ danh sách assignments đã chấm của user (không pagination)
        /// </summary>
        [HttpGet(ApiEndpoints.Jury.GetAllAssignByUserGradingAll)]
        public async Task<IActionResult> GetAllAssignByUserGradingAllAsync(int Id, [FromQuery] string? search = null)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _juryService.GetAllAssignByUserGradeAllAsync(user.Id, Id, search);
                return Ok(new ResponseDto<List<JuryAssignmentDto>>(result, "Lấy danh sách phân công giám khảo thành công"));
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
        [HttpPost(ApiEndpoints.Jury.GradeSubmission)]
        public async Task<IActionResult> GradeSubmission([FromBody] GradeSubmissionDto dto)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            try
            {
                var result = await _juryService.GradeSubmission(dto.id, user.Id, dto.Scores,dto.Comment,dto.TotalScore);
                return Ok(new ResponseDto<string>(result, "Chấm điểm thành công"));
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

        [HttpGet(ApiEndpoints.Jury.IsAssigned)]
        public async Task<IActionResult> IsAssignedToGrade(int userId, int submissionId)
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            // Chỉ cho phép user check quyền của chính họ
            if (user.Id != userId)
            {
                return BadRequest(new ResponseDto<bool>(false, "Bạn chỉ có thể kiểm tra quyền của chính mình"));
            }
            try
            {
                var result = await _juryService.IsAssignedToGrade(userId, submissionId);
                return Ok(new ResponseDto<bool>(result, result ? "Bạn được phân công chấm bài này" : "Bạn không được phân công chấm bài này"));
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

        [HttpPost(ApiEndpoints.Jury.ImprovedRandomAssign)]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ImprovedRandomAssign([FromBody] RamdomAssignJuryDto dto)
        {
            try
            {
                var result = await _juryService.ImprovedRandomAssignAsync(dto.ActivityId, dto.NumberOfJury);
                return Ok(new ResponseDto<bool>(result, "Phân công giám khảo ngẫu nhiên (cải tiến) thành công"));
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
