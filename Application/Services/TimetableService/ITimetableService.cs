using System.Collections.Generic;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.TimetableDto;

namespace EduShpere.Application.Services;

public interface ITimetableService
{
    Task<TimetableImportResultDto> ImportTimetableAsync(TimetableImportDto dto, int userId);
    Task<List<TimetableDto>> GetTimetablesByClassGroupAsync(int classGroupId);
    Task<bool> DeleteTimetablesByClassGroupAsync(int classGroupId, int userId);
}


