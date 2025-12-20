using EduShpere.Application;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.Services;
using EduShpere.Middlewares;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduShpere.Controllers;

[CustomModelValidationFilter]
public class ActivityTemplateController : BaseController
{
    private readonly IActivityTemplateService _service;
    private readonly IHttpContextService _httpContextService;

    public ActivityTemplateController(
        IActivityTemplateService service,
        IHttpContextService httpContextService)
    {
        _service = service;
        _httpContextService = httpContextService;
    }

    /// <summary>
    /// Lấy tất cả templates
    /// </summary>
    [HttpGet(ApiEndpoints.ActivityTemplate.GetAll)]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            return Ok(new ResponseDto<IEnumerable<ActivityTemplateDto>>(
                result,
                "Lấy danh sách mẫu hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Lấy template theo ID
    /// </summary>
    [HttpGet(ApiEndpoints.ActivityTemplate.GetById)]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ResponseDto<string>(
                    null,
                    "Mẫu hoạt động không tồn tại",
                    (int)HttpStatusCode.NotFound
                ));
            }

            return Ok(new ResponseDto<ActivityTemplateDto>(
                result,
                "Lấy mẫu hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Lấy templates theo SubType
    /// </summary>
    [HttpGet(ApiEndpoints.ActivityTemplate.GetBySubType)]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> GetBySubType([FromQuery] string subType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(subType))
            {
                return BadRequest(new ResponseDto<string>(
                    null,
                    "SubType không được để trống",
                    (int)HttpStatusCode.BadRequest
                ));
            }

            var result = await _service.GetBySubTypeAsync(subType);
            return Ok(new ResponseDto<IEnumerable<ActivityTemplateDto>>(
                result,
                "Lấy danh sách mẫu hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Tạo template mới
    /// </summary>
    [HttpPost(ApiEndpoints.ActivityTemplate.Create)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateActivityTemplateDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return Ok(new ResponseDto<ActivityTemplateDto>(
                result,
                "Tạo mẫu hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                ex.Message,
                (int)HttpStatusCode.BadRequest
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Lưu template từ form data (thay thế cho Draft)
    /// </summary>
    [HttpPost(ApiEndpoints.ActivityTemplate.SaveFromForm)]
    [Authorize(Roles = "Teacher,Staff,Admin")]
    public async Task<IActionResult> SaveFromForm([FromBody] CreateActivityTemplateFromFormDto dto)
    {
        try
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ResponseDto<string>(null, "Unauthorized", 401));

            var result = await _service.CreateFromFormDataAsync(dto);
            return Ok(new ResponseDto<ActivityTemplateDto>(
                result,
                "Lưu mẫu hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                ex.Message,
                (int)HttpStatusCode.BadRequest
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Cập nhật template
    /// </summary>
    [HttpPut(ApiEndpoints.ActivityTemplate.Update)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateActivityTemplateDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(dto);
            return Ok(new ResponseDto<ActivityTemplateDto>(
                result,
                "Cập nhật mẫu hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ResponseDto<string>(
                null,
                ex.Message,
                (int)HttpStatusCode.NotFound
            ));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                ex.Message,
                (int)HttpStatusCode.BadRequest
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Xóa template (soft delete)
    /// </summary>
    [HttpDelete(ApiEndpoints.ActivityTemplate.Delete)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ResponseDto<string>(
                    null,
                    "Mẫu hoạt động không tồn tại",
                    (int)HttpStatusCode.NotFound
                ));
            }

            return Ok(new ResponseDto<string>(
                null,
                "Xóa mẫu hoạt động thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                ex.Message,
                (int)HttpStatusCode.BadRequest
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Tăng số lần sử dụng template (gọi khi apply template)
    /// </summary>
    [HttpPost(ApiEndpoints.ActivityTemplate.IncrementUsage)]
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> IncrementUsage(int id)
    {
        try
        {
            await _service.IncrementUsageCountAsync(id);
            return Ok(new ResponseDto<string>(
                null,
                "Cập nhật số lần sử dụng thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(
                null,
                $"Lỗi: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }
}

