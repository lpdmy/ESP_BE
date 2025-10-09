using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application;
using EduShpere.Application.Services.ClassGroupService;
using EduShpere.Middlewares;
using EduShpere.Shared.Constants;

namespace EduShpere.Controllers;

[ApiController]
[CustomModelValidationFilter]
public class ClassGroupController : BaseController
{
    private readonly IClassGroupService _classGroupService;

    public ClassGroupController(IClassGroupService classGroupService)
    {
        _classGroupService = classGroupService;
    }

    [HttpGet(ApiEndpoints.ClassGroup.ClassGroups)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequestDto paginationRequest)
    {
        var result = await _classGroupService.GetAllAsync(paginationRequest);
        return Ok(new ResponseDto<PaginationResponseDto<ClassGroupDto>>(result, "Lấy danh sách lớp học thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.GetClassGroupById)]
    //[Authorize(Roles = "Admin,Teacher,Student")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _classGroupService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new ResponseDto<string>(null, "Không tìm thấy lớp học", 404));
        }
        return Ok(new ResponseDto<ClassGroupDto>(result, "Lấy thông tin lớp học thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.GetByName)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetByName([FromQuery] string name)
    {
        var result = await _classGroupService.GetByNameAsync(name);
        if (result == null)
        {
            return NotFound(new ResponseDto<string>(null, "Không tìm thấy lớp học", 404));
        }
        return Ok(new ResponseDto<ClassGroupDto>(result, "Lấy thông tin lớp học thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.Dashboard)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetDashboardData()
    {
        var result = await _classGroupService.GetDashboardDataAsync();
        return Ok(new ResponseDto<ClassGroupDashboardDto>(result, "Lấy dữ liệu dashboard thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.ByGrade)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetByGrade(int grade)
    {
        var filter = new ClassGroupFilterDto { Grade = grade };
        var result = await _classGroupService.GetClassesByFilterAsync(filter);
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học theo khối thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.WithoutGrade)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetWithoutGrade()
    {
        var result = await _classGroupService.GetWithoutGradeAsync();
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học không có khối thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.ByStartYear)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetByStartYear(int startYear)
    {
        var filter = new ClassGroupFilterDto { StartYear = startYear };
        var result = await _classGroupService.GetClassesByFilterAsync(filter);
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học theo năm bắt đầu thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.WithoutStartYear)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetWithoutStartYear()
    {
        var result = await _classGroupService.GetWithoutStartYearAsync();
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học không có năm bắt đầu thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.Deleted)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDeletedClasses()
    {
        var filter = new ClassGroupFilterDto { IsDeleted = true };
        var result = await _classGroupService.GetClassesByFilterAsync(filter);
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học đã xóa thành công"));
    }

    [HttpPost(ApiEndpoints.ClassGroup.Filter)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetClassesByFilter([FromBody] ClassGroupFilterDto filter)
    {
        var result = await _classGroupService.GetClassesByFilterAsync(filter);
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học theo bộ lọc thành công"));
    }

    [HttpPost(ApiEndpoints.ClassGroup.Create)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateClassGroupDto dto)
    {
        var result = await _classGroupService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
            new ResponseDto<ClassGroupDto>(result, "Tạo lớp học thành công"));
    }

    [HttpPut(ApiEndpoints.ClassGroup.Update)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassGroupDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(new ResponseDto<string>(null, "ID không khớp", 400));
        }

        var result = await _classGroupService.UpdateAsync(dto);
        return Ok(new ResponseDto<ClassGroupDto>(result, "Cập nhật lớp học thành công"));
    }

    [HttpDelete(ApiEndpoints.ClassGroup.Delete)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _classGroupService.DeleteAsync(id);
        if (!result)
        {
            return NotFound(new ResponseDto<string>(null, "Không tìm thấy lớp học", 404));
        }
        return Ok(new ResponseDto<string>(null, "Xóa lớp học thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.CheckNameExists)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> CheckNameExists([FromQuery] string name, [FromQuery] int? excludeId = null)
    {
        var result = await _classGroupService.IsNameExistsAsync(name, excludeId);
        return Ok(new ResponseDto<bool>(result, "Kiểm tra tên lớp học thành công"));
    }
}
