using EduShpere.Application.DTOs.StatisticsReportDto;

namespace EduShpere.Application.Services.StatisticsReportService;

public interface IStatisticsReportService
{
    Task<AcademicYearStatisticsReport> GetAcademicYearReportAsync(StatisticsReportRequest request);
}

