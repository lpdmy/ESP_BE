using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.AuthDto;
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
    public async Task<IActionResult> GetDashboardData([FromQuery] int? academicYearId = null)
    {
        var result = await _classGroupService.GetDashboardDataAsync(academicYearId);
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

    [HttpGet(ApiEndpoints.ClassGroup.ByAcademicYear)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetByAcademicYear(int academicYearId)
    {
        var filter = new ClassGroupFilterDto { AcademicYearId = academicYearId };
        var result = await _classGroupService.GetClassesByFilterAsync(filter);
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học theo năm học thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.WithoutAcademicYear)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetWithoutAcademicYear()
    {
        var result = await _classGroupService.GetWithoutAcademicYearAsync();
        return Ok(new ResponseDto<IEnumerable<ClassGroupDto>>(result, "Lấy danh sách lớp học không có năm học thành công"));
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
    public async Task<IActionResult> CheckNameExists([FromQuery] string name, [FromQuery] int? grade = null, [FromQuery] int? academicYearId = null, [FromQuery] int? excludeId = null)
    {
        var result = await _classGroupService.IsExistsAsync(name, grade, academicYearId, excludeId);
        return Ok(new ResponseDto<bool>(result, "Kiểm tra lớp học theo tổ hợp thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.GetClassGroupDetail)]
    //[Authorize(Roles = "Admin,Teacher,Student")]
    public async Task<IActionResult> GetDetailById(int id)
    {
        var result = await _classGroupService.GetDetailByIdAsync(id);
        if (result == null)
        {
            return NotFound(new ResponseDto<string>(null, "Không tìm thấy lớp học", 404));
        }
        return Ok(new ResponseDto<ClassGroupDetailDto>(result, "Lấy thông tin chi tiết lớp học thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.GetStudents)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetStudents(int id)
    {
        var result = await _classGroupService.GetStudentsInClassAsync(id);
        return Ok(new ResponseDto<IEnumerable<ClassGroupStudentDto>>(result, "Lấy danh sách học sinh thành công"));
    }

    [HttpPost(ApiEndpoints.ClassGroup.AddStudent)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddStudent(int id, [FromBody] AddStudentToClassDto dto)
    {
        var result = await _classGroupService.AddStudentToClassAsync(id, dto);
        if (!result.Success)
        {
            return BadRequest(new ResponseDto<AddStudentToClassResponseDto>(result, result.Message, 400));
        }
        return Ok(new ResponseDto<AddStudentToClassResponseDto>(result, result.Message));
    }

    [HttpDelete(ApiEndpoints.ClassGroup.RemoveStudent)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveStudent(int id, int studentId)
    {
        var result = await _classGroupService.RemoveStudentFromClassAsync(id, studentId);
        if (!result)
        {
            return BadRequest(new ResponseDto<string>(null, "Không thể xóa học sinh khỏi lớp", 400));
        }
        return Ok(new ResponseDto<string>(null, "Xóa học sinh khỏi lớp thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.GetAcademicYears)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAcademicYears()
    {
        var result = await _classGroupService.GetAllAcademicYearsAsync();
        return Ok(new ResponseDto<IEnumerable<AcademicYearDto>>(result, "Lấy danh sách niên khóa thành công"));
    }

    [HttpPut(ApiEndpoints.ClassGroup.AssignHomeroomTeacher)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignHomeroomTeacher(int id, [FromBody] AssignHomeroomTeacherDto dto)
    {
        var result = await _classGroupService.AssignHomeroomTeacherAsync(id, dto);
        if (!result.Success)
        {
            return BadRequest(new ResponseDto<AssignHomeroomTeacherResponseDto>(result, result.Message, 400));
        }
        return Ok(new ResponseDto<AssignHomeroomTeacherResponseDto>(result, result.Message));
    }

    [HttpDelete(ApiEndpoints.ClassGroup.RemoveHomeroomTeacher)]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveHomeroomTeacher(int id)
    {
        var result = await _classGroupService.RemoveHomeroomTeacherAsync(id);
        if (!result)
        {
            return BadRequest(new ResponseDto<string>(null, "Không thể bỏ gán giáo viên chủ nhiệm", 400));
        }
        return Ok(new ResponseDto<string>(null, "Bỏ gán giáo viên chủ nhiệm thành công"));
    }

    [HttpGet(ApiEndpoints.ClassGroup.GetHomeroomTeacher)]
    //[Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetHomeroomTeacher(int id)
    {
        var result = await _classGroupService.GetHomeroomTeacherAsync(id);
        if (result == null)
        {
            return NotFound(new ResponseDto<string>(null, "Không tìm thấy giáo viên chủ nhiệm", 404));
        }
        return Ok(new ResponseDto<UserDto>(result, "Lấy thông tin giáo viên chủ nhiệm thành công"));
    }
}
