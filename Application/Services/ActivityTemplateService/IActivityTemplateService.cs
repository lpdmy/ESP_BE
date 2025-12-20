using EduShpere.Application.DTOs.ActivityDto;

namespace EduShpere.Application.Services;

public interface IActivityTemplateService
{
    Task<IEnumerable<ActivityTemplateDto>> GetAllAsync();
    Task<ActivityTemplateDto?> GetByIdAsync(int id);
    Task<IEnumerable<ActivityTemplateDto>> GetBySubTypeAsync(string subType);
    Task<ActivityTemplateDto> CreateAsync(CreateActivityTemplateDto dto);
    Task<ActivityTemplateDto> CreateFromFormDataAsync(CreateActivityTemplateFromFormDto dto);
    Task<ActivityTemplateDto> UpdateAsync(UpdateActivityTemplateDto dto);
    Task<bool> DeleteAsync(int id);
    Task IncrementUsageCountAsync(int id);
}

