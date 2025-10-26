using AutoMapper;
using EduShpere.Application;
using EduShpere.Application.DTOs.AttachmentDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SystemAnnouncementDto;
using EduShpere.Application.Services.SystemAnnouncementService;
using EduShpere.Middlewares;
using EduShpere.Shared.Constants;
using EduShpere.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EduShpere.Controllers;

[ApiController]
[Route("api/[controller]")]
[CustomModelValidationFilter]
public class SystemAnnouncementController : BaseController
{
    private readonly ISystemAnnouncementService _systemAnnouncementService;
    private readonly IMapper _mapper;

    public SystemAnnouncementController(
        ISystemAnnouncementService systemAnnouncementService,
        IMapper mapper)
    {
        _systemAnnouncementService = systemAnnouncementService;
        _mapper = mapper;
    }

    /// <summary>
    /// Lấy danh sách thông báo hệ thống với pagination (Admin only)
    /// </summary>
    [HttpGet]
    // [Authorize(Roles = "Admin")] // Tạm thời comment để test
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequestDto paginationRequest)
    {
        try
        {
            var result = await _systemAnnouncementService.GetAllAsync(paginationRequest);
            return Ok(new ResponseDto<PaginationResponseDto<SystemAnnouncementListItemDto>>(result, "Lấy danh sách thông báo hệ thống thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    /// <summary>
    /// Lấy chi tiết thông báo hệ thống theo ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _systemAnnouncementService.GetByIdAsync(id);
            
            if (result == null)
                return NotFound(new ResponseDto<string>(null, ErrorMessages.SystemAnnouncement.NotFound, 404));

            return Ok(new ResponseDto<SystemAnnouncementDetailDto>(result, "Lấy thông tin thông báo thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    /// <summary>
    /// Tạo thông báo hệ thống mới (Admin only)
    /// </summary>
    [HttpPost]
    // [Authorize(Roles = "Admin")] // Tạm thời comment để test
    public async Task<IActionResult> Create([FromForm] CreateSystemAnnouncementDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _systemAnnouncementService.CreateAsync(dto, userId);
            
            return Ok(new ResponseDto<SystemAnnouncementDetailDto>(result, "Tạo thông báo hệ thống thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    /// <summary>
    /// Cập nhật thông báo hệ thống (Admin only)
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromForm] UpdateSystemAnnouncementDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _systemAnnouncementService.UpdateAsync(dto, userId);
            
            return Ok(new ResponseDto<SystemAnnouncementDetailDto>(result, "Cập nhật thông báo hệ thống thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    /// <summary>
    /// Xóa thông báo hệ thống (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _systemAnnouncementService.DeleteAsync(id);
            
            if (!result)
                return NotFound(new ResponseDto<string>(null, ErrorMessages.SystemAnnouncement.NotFound, 404));

            return Ok(new ResponseDto<bool>(true, "Xóa thông báo hệ thống thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    /// <summary>
    /// Ẩn/hiện thông báo hệ thống (Admin only)
    /// </summary>
    [HttpPut("{id}/toggle-visibility")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleVisibility(int id)
    {
        try
        {
            var result = await _systemAnnouncementService.ToggleVisibilityAsync(id);
            
            if (!result)
                return NotFound(new ResponseDto<string>(null, ErrorMessages.SystemAnnouncement.NotFound, 404));

            return Ok(new ResponseDto<bool>(true, "Cập nhật trạng thái thông báo thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    /// <summary>
    /// Lấy danh sách thông báo công khai cho user
    /// </summary>
    [HttpGet("public")]
    // [Authorize(Roles = "Student,Teacher,Admin")] // Tạm thời comment để test
    public async Task<IActionResult> GetPublicAnnouncements()
    {
        try
        {
            var result = await _systemAnnouncementService.GetPublicAnnouncementsAsync();
            return Ok(new ResponseDto<List<SystemAnnouncementDto>>(result, "Lấy danh sách thông báo công khai thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    /// <summary>
    /// Đánh dấu thông báo đã xem
    /// </summary>
    [HttpPost("{id}/mark-viewed")]
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> MarkAsViewed(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _systemAnnouncementService.MarkAsViewedAsync(id, userId);
            
            return Ok(new ResponseDto<bool>(result, "Đánh dấu đã xem thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<string>(null, ex.Message, 400));
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Console.WriteLine($"Debug - UserIdClaim: {userIdClaim}");
        Console.WriteLine($"Debug - User.Identity.Name: {User.Identity?.Name}");
        Console.WriteLine($"Debug - User Claims Count: {User.Claims.Count()}");
        
        if (int.TryParse(userIdClaim, out int userId))
            return userId;
        
        // Tạm thời return 1 để test
        Console.WriteLine("Warning: Using mock userId = 1 for testing");
        return 1;
    }
}
