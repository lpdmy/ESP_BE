using EduShpere.Application.DTOs.ClassGroupDto;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.Services.ClassGroupService;

public interface IClassGroupService
{
    Task<PaginationResponseDto<ClassGroupDto>> GetAllAsync(PaginationRequestDto paginationRequest);
    Task<ClassGroupDto?> GetByIdAsync(int id);
    Task<ClassGroupDto?> GetByNameAsync(string name);
    Task<ClassGroupDashboardDto> GetDashboardDataAsync();
    Task<ClassGroupDto> CreateAsync(CreateClassGroupDto dto);
    Task<ClassGroupDto> UpdateAsync(UpdateClassGroupDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
    Task<IEnumerable<ClassGroupDto>> GetClassesByFilterAsync(ClassGroupFilterDto filter);
    Task<IEnumerable<ClassGroupDto>> GetWithoutStartYearAsync();
    Task<IEnumerable<ClassGroupDto>> GetWithoutGradeAsync();
}
