using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.Services
{
    public interface IActivityDraftService
    {
        Task<ActivityDraftResponseDto> CreateAsync(CreateActivityDraftDto dto, int userId);
        Task<ActivityDraftResponseDto> UpdateAsync(UpdateActivityDraftDto dto, int userId);
        Task<ActivityDraftResponseDto> GetByIdAsync(int id, int userId);
        Task<PaginationResponseDto<ActivityDraftListItemDto>> GetAllByUserIdAsync(PaginationRequestDto paginationRequest, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<CreateActivityDto> ConvertDraftToActivityDtoAsync(int draftId, int userId);
    }
}

