using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IActivityService
    {
        Task<(IEnumerable<Activity> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? search = null);
        Task<IEnumerable<ActivityListItemDto>> GetListItemsAsync();
        Task<PaginationResponseDto<ActivityListItemDto>> GetListItemsWithFilterAsync(ActivityListFilterDto filter, int? userId = null);
        Task<Activity?> GetByIdAsync(int id);
        Task<ActivityResponseDto> AddAsync(CreateActivityDto dto);
        Task<ActivityResponseDto> UpdateAsync(UpdateActivityDto dto);
        Task<(IEnumerable<Activity> Items, int TotalCount)> GetActivitiesByUserIdAsync(int userId, int pageNumber, int pageSize, string? search = null, string? status = null);
        Task<ActivityStatisticsDto> GetStatisticsAsync();
    }
}
