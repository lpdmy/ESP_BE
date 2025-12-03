using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IJuryService
    {
        Task<List<JuryActivityResponseDto>> GetAllJuryByActivityIdAsync(int activityId, string? search);
        Task<List<JuryActivityResponseDto>> CreateJury(CreateJuryDto dto);
        Task DeletedJury(int id);
        Task<List<JuryAssignmentDto>> AssignJury(AssignJuryDto dto);
        Task<PaginationResponseDto<JuryActivityResponseDto>> GetAllByUserAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<bool> AutoAssignRandomAsync(int activityId, int juryPerSubmission);
        Task<bool> DeleteAssignJury(int activityId);
        Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserAsync(int userid, int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserNotGradeAsync(int userid, int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<PaginationResponseDto<JuryAssignmentDto>> GetAllAssignByUserGradeAsync(int userid, int activityId,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<string> GradeSubmission(int assignmentId, Dictionary<string, int> scores, string? comment,int totalScore);
    }
}
