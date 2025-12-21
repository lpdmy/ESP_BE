using EduShpere.Application;
using EduShpere.Application.DTOs.StatisticsDto;
using EduShpere.Application.Services.StatisticsService;
using EduShpere.Shared.Constants;
using EduShpere.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EduShpere.Controllers;

[CustomModelValidationFilter]
public class StatisticsController : BaseController
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Lấy thống kê tổng quan về sự kiện
    /// </summary>
    [HttpGet(ApiEndpoints.Statistics.ActivityOverview)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetActivityOverviewStatistics([FromQuery] int? academicYearId = null)
    {
        try
        {
            var result = await _statisticsService.GetActivityOverviewStatisticsAsync(academicYearId);
            return Ok(new ResponseDto<ActivityOverviewStatisticsDto>(
                result,
                "Lấy thống kê tổng quan sự kiện thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<ActivityOverviewStatisticsDto>(
                null!,
                $"Lỗi khi lấy thống kê: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Lấy thống kê chi tiết của 1 sự kiện cụ thể
    /// </summary>
    [HttpGet(ApiEndpoints.Statistics.ActivityDetail)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetActivityDetailStatistics([FromRoute] int activityId)
    {
        try
        {
            var result = await _statisticsService.GetActivityDetailStatisticsAsync(activityId);
            return Ok(new ResponseDto<ActivityDetailStatisticsDto>(
                result,
                "Lấy thống kê chi tiết sự kiện thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto<ActivityDetailStatisticsDto>(
                null!,
                ex.Message,
                (int)HttpStatusCode.NotFound
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<ActivityDetailStatisticsDto>(
                null!,
                $"Lỗi khi lấy thống kê: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Lấy thống kê theo năm học
    /// </summary>
    [HttpGet(ApiEndpoints.Statistics.AcademicYear)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetAcademicYearStatistics([FromRoute] int academicYearId)
    {
        try
        {
            var result = await _statisticsService.GetAcademicYearStatisticsAsync(academicYearId);
            return Ok(new ResponseDto<AcademicYearStatisticsDto>(
                result,
                "Lấy thống kê năm học thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto<AcademicYearStatisticsDto>(
                null!,
                ex.Message,
                (int)HttpStatusCode.NotFound
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<AcademicYearStatisticsDto>(
                null!,
                $"Lỗi khi lấy thống kê: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Lấy thống kê theo lớp
    /// </summary>
    [HttpGet(ApiEndpoints.Statistics.ClassGroup)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetClassGroupStatistics(
        [FromRoute] int classGroupId,
        [FromQuery] int? academicYearId = null)
    {
        try
        {
            var result = await _statisticsService.GetClassGroupStatisticsAsync(classGroupId, academicYearId);
            return Ok(new ResponseDto<ClassGroupStatisticsDto>(
                result,
                "Lấy thống kê lớp thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ResponseDto<ClassGroupStatisticsDto>(
                null!,
                ex.Message,
                (int)HttpStatusCode.NotFound
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<ClassGroupStatisticsDto>(
                null!,
                $"Lỗi khi lấy thống kê: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }

    /// <summary>
    /// Lấy Dashboard tổng hợp cho Admin
    /// </summary>
    [HttpGet(ApiEndpoints.Statistics.Dashboard)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDashboardStatistics([FromQuery] int? academicYearId = null)
    {
        try
        {
            var result = await _statisticsService.GetDashboardStatisticsAsync(academicYearId);
            return Ok(new ResponseDto<DashboardStatisticsDto>(
                result,
                "Lấy thống kê dashboard thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new ResponseDto<DashboardStatisticsDto>(
                null!,
                $"Lỗi khi lấy thống kê: {ex.Message}",
                (int)HttpStatusCode.BadRequest
            ));
        }
    }
}

