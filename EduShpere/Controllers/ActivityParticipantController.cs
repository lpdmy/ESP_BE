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
        public ActivityParticipantController(IActivityParticipantService service)
        {
            _service = service;
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
        [HttpDelete(ApiEndpoints.ActivityParticipant.ActivityParticipantRoute)]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> RemoveActivityParticipant( int participationId)
        {
            if (participationId <= 0)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            var result = await _service.RemoveActivityParticipant(participationId);
            return Ok(new ResponseDto<ActivityParticipantResponseDto>(result, "Xóa người tham gia hoạt động thành công", 200));
        }
    }
}
