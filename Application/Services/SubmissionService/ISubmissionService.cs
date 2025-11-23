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
        Task<SubmissionResponseDto> CreateSubmission(CreateSubmissionDto dto, int userId);
        Task<SubmissionResponseDto?> GetSubmissionById(int id);
        Task<PaginationResponseDto<SubmissionResponseDto>> GetMySubmissions(int userId, PaginationRequestDto paginationRequest, string? search = null);
        Task<SubmissionResponseDto?> GetMySubmissionByActivityId(int activityId, int userId);
        Task<SubmissionResponseDto> UpdateSubmission(UpdateSubmissionDto dto, int userId);
        Task<bool> DeleteSubmission(int id, int userId);
    }
}
