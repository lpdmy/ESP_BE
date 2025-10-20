using EduShpere.Application.DTOs.AuthDto;
using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.Services.ClassGroupService;

public interface IClassGroupService
{
    Task<PaginationResponseDto<ClassGroupDto>> GetAllAsync(PaginationRequestDto paginationRequest);
    Task<ClassGroupDto?> GetByIdAsync(int id);
    Task<ClassGroupDetailDto?> GetDetailByIdAsync(int id);
    Task<ClassGroupDto?> GetByNameAsync(string name);
    Task<ClassGroupDashboardDto> GetDashboardDataAsync(int? academicYearId = null);
    Task<ClassGroupDto> CreateAsync(CreateClassGroupDto dto);
    Task<ClassGroupDto> UpdateAsync(UpdateClassGroupDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
    Task<bool> IsExistsAsync(string name, int? grade, int? academicYearId, int? excludeId = null);
    Task<IEnumerable<ClassGroupDto>> GetClassesByFilterAsync(ClassGroupFilterDto filter);
    Task<IEnumerable<ClassGroupDto>> GetWithoutAcademicYearAsync();
    Task<IEnumerable<ClassGroupDto>> GetWithoutGradeAsync();
    
    // Student management methods
    Task<IEnumerable<ClassGroupStudentDto>> GetStudentsInClassAsync(int classGroupId);
    Task<AddStudentToClassResponseDto> AddStudentToClassAsync(int classGroupId, AddStudentToClassDto dto);
    Task<bool> RemoveStudentFromClassAsync(int classGroupId, int studentId);
    
    // Academic Year methods
    Task<IEnumerable<AcademicYearDto>> GetAllAcademicYearsAsync();
    
    // Homeroom Teacher management methods
    Task<AssignHomeroomTeacherResponseDto> AssignHomeroomTeacherAsync(int classGroupId, AssignHomeroomTeacherDto dto);
    Task<bool> RemoveHomeroomTeacherAsync(int classGroupId);
    Task<UserDto?> GetHomeroomTeacherAsync(int classGroupId);
}
