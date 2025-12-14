using EduShpere.Application;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.Services;
using EduShpere.Middlewares;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    [ApiController]
    [Route("api/activity-drafts")]
    public class ActivityDraftController : BaseController
    {
        private readonly IActivityDraftService _service;
        private readonly IActivityService _activityService;
        private readonly IHttpContextService _httpContextService;

        public ActivityDraftController(
            IActivityDraftService service,
            IActivityService activityService,
            IHttpContextService httpContextService)
        {
            _service = service;
            _activityService = activityService;
            _httpContextService = httpContextService;
        }

        [HttpPost]
        [Authorize(Roles = "Teacher,Staff,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateActivityDraftDto dto)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));

            var result = await _service.CreateAsync(dto, userId.Value);
            return Ok(new ResponseDto<ActivityDraftResponseDto>(result, "Lưu bản nháp thành công", (int)HttpStatusCode.OK));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Staff,Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateActivityDraftDto dto)
        {
            dto.Id = id;
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));

            var result = await _service.UpdateAsync(dto, userId.Value);
            return Ok(new ResponseDto<ActivityDraftResponseDto>(result, "Cập nhật bản nháp thành công", (int)HttpStatusCode.OK));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Teacher,Staff,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));

            var result = await _service.GetByIdAsync(id, userId.Value);
            return Ok(new ResponseDto<ActivityDraftResponseDto>(result, "Lấy bản nháp thành công", (int)HttpStatusCode.OK));
        }

        [HttpGet]
        [Authorize(Roles = "Teacher,Staff,Admin")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "UpdatedAt",
            [FromQuery] bool sortDescending = true)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));

            var paginationRequest = new PaginationRequestDto
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _service.GetAllByUserIdAsync(paginationRequest, userId.Value);
            return Ok(new ResponseDto<PaginationResponseDto<ActivityDraftListItemDto>>(result, "Lấy danh sách bản nháp thành công", (int)HttpStatusCode.OK));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher,Staff,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));

            await _service.DeleteAsync(id, userId.Value);
            return Ok(new ResponseDto<string>(null, "Xóa bản nháp thành công", (int)HttpStatusCode.OK));
        }

        [HttpPost("{id}/convert-to-activity")]
        [Authorize(Roles = "Teacher,Staff,Admin")]
        public async Task<IActionResult> ConvertToActivity(int id)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));

            var activityDto = await _service.ConvertDraftToActivityDtoAsync(id, userId.Value);
            var result = await _activityService.AddAsync(activityDto);
            
            return Ok(new ResponseDto<ActivityResponseDto>(result, "Tạo hoạt động từ bản nháp thành công", (int)HttpStatusCode.OK));
        }

    }
}

