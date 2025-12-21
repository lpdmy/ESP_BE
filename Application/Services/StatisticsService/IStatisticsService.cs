using EduShpere.Application.DTOs.StatisticsDto;

namespace EduShpere.Application.Services.StatisticsService;

public interface IStatisticsService
{
    /// <summary>
    /// Lấy thống kê tổng quan về sự kiện
    /// </summary>
    Task<ActivityOverviewStatisticsDto> GetActivityOverviewStatisticsAsync(int? academicYearId = null);
    
    /// <summary>
    /// Lấy thống kê chi tiết của 1 sự kiện cụ thể
    /// </summary>
    Task<ActivityDetailStatisticsDto> GetActivityDetailStatisticsAsync(int activityId);
    
    /// <summary>
    /// Lấy thống kê theo năm học
    /// </summary>
    Task<AcademicYearStatisticsDto> GetAcademicYearStatisticsAsync(int academicYearId);
    
    /// <summary>
    /// Lấy thống kê theo lớp
    /// </summary>
    Task<ClassGroupStatisticsDto> GetClassGroupStatisticsAsync(int classGroupId, int? academicYearId = null);
    
    /// <summary>
    /// Lấy Dashboard tổng hợp cho Admin
    /// </summary>
    Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(int? academicYearId = null);
}
