using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Middlewares;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class ActivityParticipantController : BaseController
    {
        private readonly IActivityParticipantService _service;
        private readonly IHttpContextService _httpContextService;
        public ActivityParticipantController(IActivityParticipantService service, IHttpContextService httpContextService)
        {
            _service = service;
            _httpContextService = httpContextService;
        }
        [HttpPost(ApiEndpoints.ActivityParticipant.ActivityParticipantRoute)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> AddActivityParticipant([FromBody] AddParticipantDto dto)
        {
            if (dto == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            var result = await _service.AddActivityParticipant(dto);
            return Ok(new ResponseDto<ActivityParticipantResponseDto>(result, "Thêm người tham gia hoạt động thành công",200));
        }

        [HttpPost(ApiEndpoints.ActivityParticipant.GroupRegistration)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> RegisterGroup([FromBody] GroupRegistrationDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            }

            var result = await _service.RegisterGroupAsync(dto);
            return Ok(new ResponseDto<GroupRegistrationResultDto>(result, "Đăng ký nhóm tham gia hoạt động thành công", 200));
        }

        [HttpPost(ApiEndpoints.ActivityParticipant.SportRegistration)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> RegisterSport([FromBody] SportRegistrationDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            }

            var result = await _service.RegisterSportAsync(dto);
            return Ok(new ResponseDto<SportRegistrationResultDto>(result, "Đăng ký hội thao thành công", 200));
        }
        [HttpDelete(ApiEndpoints.ActivityParticipant.ActivityParticipantRoute)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> RemoveActivityParticipant( int participationId)
        {
            if (participationId <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            var result = await _service.RemoveActivityParticipant(participationId);
            return Ok(new ResponseDto<ActivityParticipantResponseDto>(result, "Xóa người tham gia hoạt động thành công", 200));
        }

        [HttpDelete(ApiEndpoints.ActivityParticipant.CancelRegistration)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> CancelRegistration([FromRoute] int activityId)
        {
            if (activityId <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
            {
                // Trả về 400 thay vì 401 để tránh frontend tự động xóa token và redirect
                return BadRequest(new ResponseDto<string>(null, "Không thể xác định người dùng. Vui lòng đăng nhập lại.", 400));
            }

            var result = await _service.CancelRegistrationAsync(activityId, userId.Value);
            return Ok(new ResponseDto<bool>(result, "Hủy đăng ký tham gia hoạt động thành công", 200));
        }

        [HttpGet(ApiEndpoints.ActivityParticipant.GetSportRosters)]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetSportRosters([FromQuery] SportRosterPaginationRequestDto request)
        {
            if (request == null || request.ActivityId <= 0)
            {
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            }

            var result = await _service.GetSportRostersAsync(request);
            return Ok(new ResponseDto<SportRosterPaginationResponseDto>(result, "Lấy danh sách đội hình thành công", 200));
        }
    }
}
