using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.ModerationDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IModerationService
    {
        Task CreateReport(AddModerationDto dto);
        Task<PaginationResponseDto<ModerationResponseDto>> GetAllModeration(
   PaginationRequestDto paginationRequest,
   string? search = null);
        Task CreateReportAsync(CreateReportDTO dto, User user);
        Task<PaginationResponseDto<UserViolationStat>> GetAllUserModeration(
   PaginationRequestDto paginationRequest,
   string? search = null);
        Task SendWarningNotification(int UserId, string message);
        Task UpdateStatus(int id, int status);
    }
}
