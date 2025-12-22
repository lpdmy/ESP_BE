using EduShpere.Application;
using EduShpere.Application.DTOs.StatisticsReportDto;
using EduShpere.Application.Services.StatisticsReportService;
using EduShpere.Shared.Constants;
using EduShpere.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mime;

namespace EduShpere.Controllers;

[CustomModelValidationFilter]
[ApiController]
[Route("api/statistics/report")]
public class StatisticsReportController : BaseController
{
    private readonly IStatisticsReportService _statisticsReportService;
    private readonly IStatisticsReportExportService _exportService;

    public StatisticsReportController(
        IStatisticsReportService statisticsReportService,
        IStatisticsReportExportService exportService)
    {
        _statisticsReportService = statisticsReportService;
        _exportService = exportService;
    }

    [HttpPost(ApiEndpoints.Statistics.AcademicYearReport)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetAcademicYearReport([FromBody] StatisticsReportRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AcademicYear))
            {
                return BadRequest(new ResponseDto<AcademicYearStatisticsReport>(
                    null!,
                    "Năm học là bắt buộc",
                    (int)HttpStatusCode.BadRequest
                ));
            }

            var result = await _statisticsReportService.GetAcademicYearReportAsync(request);
            
            return Ok(new ResponseDto<AcademicYearStatisticsReport>(
                result,
                "Lấy báo cáo thống kê năm học thành công",
                (int)HttpStatusCode.OK
            ));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ResponseDto<AcademicYearStatisticsReport>(
                null!,
                ex.Message,
                (int)HttpStatusCode.BadRequest
            ));
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseDto<AcademicYearStatisticsReport>(
                null!,
                $"Lỗi khi lấy báo cáo thống kê: {ex.Message}",
                (int)HttpStatusCode.InternalServerError
            ));
        }
    }

    [HttpPost(ApiEndpoints.Statistics.AcademicYearReportExport)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> ExportAcademicYearReport(
        [FromBody] StatisticsReportRequest request,
        [FromQuery] string format = "pdf")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AcademicYear))
            {
                return BadRequest(new ResponseDto<string>(
                    null!,
                    "Năm học là bắt buộc",
                    (int)HttpStatusCode.BadRequest
                ));
            }

            var report = await _statisticsReportService.GetAcademicYearReportAsync(request);

            byte[] fileBytes;
            string contentType;
            string fileName;

            if (format.ToLower() == "excel")
            {
                fileBytes = await _exportService.ExportToExcelAsync(report, request);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"bao-cao-thong-ke-{request.AcademicYear}.xlsx";
            }
            else
            {
                fileBytes = await _exportService.ExportToPdfAsync(report, request);
                contentType = "application/pdf";
                fileName = $"bao-cao-thong-ke-{request.AcademicYear}.pdf";
            }

            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            return File(fileBytes, contentType, fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ResponseDto<string>(
                null!,
                ex.Message,
                (int)HttpStatusCode.BadRequest
            ));
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new ResponseDto<string>(
                null!,
                $"Lỗi khi xuất báo cáo: {ex.Message}",
                (int)HttpStatusCode.InternalServerError
            ));
        }
    }
}

