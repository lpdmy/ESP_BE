using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs.SubmissionDto;

namespace EduShpere.Application.Services
{
    public interface ISubmissionService
    {
        Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityId(int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserId(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByActivityIdByUserIDGrading(
    int activityId, int userId,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<PaginationResponseDto<SubmissionResponseDto>> GetAllSubmissionByUser(
        int userId,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<List<SubmissionResponseDto>> GetRankByActivityId(int id);
        Task<SubmissionResponseDto> GetSubmissionById(int id);
        Task<SubmissionStatusDto> GetSubmissionStatusAsync(int activityId, int userId);
        Task<SubmissionResponseDto> CreateSubmissionAsync(CreateSubmissionDto dto, int userId);
        Task<SubmissionResponseDto?> GetMySubmissionAsync(int activityId, int userId);
    }
}
