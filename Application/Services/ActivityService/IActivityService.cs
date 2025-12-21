using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application.Services
{
    public interface IActivityService
    {
        Task<(IEnumerable<Activity> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? search = null);
        /// <summary>
        /// Get paginated list of activities with optimized DTO (for list view)
        /// </summary>
        Task<PaginationResponseDto<ActivityListItemDto>> GetAllOptimizedAsync(int pageNumber, int pageSize, string? search = null);
        Task<IEnumerable<ActivityListItemDto>> GetListItemsAsync();
        Task<PaginationResponseDto<ActivityListItemDto>> GetListItemsWithFilterAsync(ActivityListFilterDto filter, int? userId = null);
        Task<Activity?> GetByIdAsync(int id);
        Task<ActivityResponseDto> AddAsync(CreateActivityDto dto);
        Task<ActivityResponseDto> UpdateAsync(UpdateActivityDto dto);
        Task<(IEnumerable<Activity> Items, int TotalCount)> GetActivitiesByUserIdAsync(int userId, int pageNumber, int pageSize, string? search = null, string? status = null);
        Task<ActivityStatisticsDto> GetStatisticsAsync();
        Task<RecentActivityInputsDto> GetRecentInputsAsync(int userId, int take = 5);
        /// <summary>
        /// Quét danh sách participants đã hoàn thành hoạt động và cộng điểm tham gia
        /// </summary>
        Task<int> AwardParticipationPointsAsync(int activityId);
        /// <summary>
        /// Trao điểm thưởng cho participants dựa trên rank (ActivityReward)
        /// </summary>
        Task<bool> AwardRankRewardsAsync(int activityId, Dictionary<int, string> participantRanks);
        /// <summary>
        /// Tự động cộng điểm tham gia cho activity (dùng cho Power Automate)
        /// Trả về thông tin chi tiết về kết quả
        /// </summary>
        Task<AutoAwardPointsResponseDto> AutoAwardParticipationPointsAsync(int activityId);
        /// <summary>
        /// Batch cộng điểm cho nhiều activities đã kết thúc (dùng cho Power Automate)
        /// </summary>
        Task<BatchAutoAwardPointsResponseDto> BatchAutoAwardParticipationPointsAsync(List<int> activityIds);
        /// <summary>
        /// Tự động cộng điểm cho tất cả activities đã kết thúc nhưng chưa được cộng điểm (dùng cho Power Automate daily job)
        /// Optimized query để performance tốt
        /// </summary>
        Task<BatchAutoAwardPointsResponseDto> AutoAwardAllEndedActivitiesAsync();
    }
}
