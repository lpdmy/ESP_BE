using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.Services;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class ActivityParticipantController : ControllerBase
    {
        private readonly IActivityParticipantService _service;
        public ActivityParticipantController(IActivityParticipantService service)
        {
            _service = service;
        }
        [HttpPost(ApiEndpoints.ActivityParticipant.GetAllActivityParticipant)]
        public async Task<IActionResult> AddActivityParticipant([FromBody] AddParticipantDto dto)
        {
            if (dto == null)
                return BadRequest(new ResponseDto<string>(null, ErrorMessages.Generic.UnknownError, 400));
            var result = await _service.AddActivityParticipant(dto);
            return Ok(new ResponseDto<ActivityParticipantResponseDto>(result, "Thêm người tham gia hoạt động thành công",200));
        }
    }
}
