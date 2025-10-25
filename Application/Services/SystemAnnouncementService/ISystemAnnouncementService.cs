using EduShpere.Application;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SystemAnnouncementDto;

namespace EduShpere.Application.Services.SystemAnnouncementService;

public interface ISystemAnnouncementService
{
    Task<PaginationResponseDto<SystemAnnouncementListItemDto>> GetAllAsync(PaginationRequestDto paginationRequest);
    Task<SystemAnnouncementDetailDto?> GetByIdAsync(int id);
    Task<SystemAnnouncementDetailDto> CreateAsync(CreateSystemAnnouncementDto dto, int userId);
    Task<SystemAnnouncementDetailDto> UpdateAsync(UpdateSystemAnnouncementDto dto, int userId);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleVisibilityAsync(int id);
    Task<List<SystemAnnouncementDto>> GetPublicAnnouncementsAsync();
    Task<bool> MarkAsViewedAsync(int announcementId, int userId);
}
