using EduShpere.Application.DTOs.StatisticsReportDto;

namespace EduShpere.Application.Services.StatisticsReportService;

public interface IStatisticsReportExportService
{
    Task<byte[]> ExportToPdfAsync(AcademicYearStatisticsReport report, StatisticsReportRequest request);
    Task<byte[]> ExportToExcelAsync(AcademicYearStatisticsReport report, StatisticsReportRequest request);
}

