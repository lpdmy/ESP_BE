using EduShpere.Application.DTOs.TimetableDto;
using EduShpere.Application.Services;
using EduShpere.Middlewares;
using EduShpere.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Application;

namespace EduShpere.Controllers;

[ApiController]
[CustomModelValidationFilter]
[Authorize(Roles = "Admin,Teacher")]
public class TimetableController : BaseController
{
    private readonly ITimetableService _service;
    private readonly IHttpContextService _httpContextService;

    public TimetableController(ITimetableService service, IHttpContextService httpContextService)
    {
        _service = service;
        _httpContextService = httpContextService;
    }

    [HttpPost("api/timetable/import")]
    public async Task<IActionResult> ImportTimetable([FromForm] TimetableImportDto dto)
    {
        try
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var result = await _service.ImportTimetableAsync(dto, user.Id);
            return Ok(new ResponseDto<TimetableImportResultDto>(result, result.Message, 200));
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new ResponseDto<TimetableImportResultDto>(null, ex.Message, 400));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ResponseDto<TimetableImportResultDto>(null, ex.Message, 404));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto<TimetableImportResultDto>(null, ex.Message, 500));
        }
    }

    [HttpGet("api/timetable/classgroup/{classGroupId}")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> GetTimetablesByClassGroup(int classGroupId)
    {
        try
        {
            var result = await _service.GetTimetablesByClassGroupAsync(classGroupId);
            return Ok(new ResponseDto<List<TimetableDto>>(result, "Lấy lịch học thành công", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto<List<TimetableDto>>(null, ex.Message, 500));
        }
    }

    [HttpDelete("api/timetable/classgroup/{classGroupId}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteTimetablesByClassGroup(int classGroupId)
    {
        try
        {
            var user = await _httpContextService.GetAppUserAndThrow();
            var result = await _service.DeleteTimetablesByClassGroupAsync(classGroupId, user.Id);
            return Ok(new ResponseDto<bool>(result, "Xóa lịch học thành công", 200));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ResponseDto<bool>(false, ex.Message, 500));
        }
    }
}


