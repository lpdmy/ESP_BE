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
        
        /// <summary>
        /// Lấy toàn bộ danh sách assignments chưa chấm của user (không pagination)
        /// </summary>
        Task<List<JuryAssignmentDto>> GetAllAssignByUserNotGradeAllAsync(int userid, int activityId, string? search = null);
        
        /// <summary>
        /// Lấy toàn bộ danh sách assignments đã chấm của user (không pagination)
        /// </summary>
        Task<List<JuryAssignmentDto>> GetAllAssignByUserGradeAllAsync(int userid, int activityId, string? search = null);
        Task<string> GradeSubmission(int assignmentId, int userId, Dictionary<string, int> scores, string? comment,int totalScore);
        
        /// <summary>
        /// Check if a user is assigned to grade a specific submission
        /// </summary>
        Task<bool> IsAssignedToGrade(int userId, int submissionId);
        
        /// <summary>
        /// Improved random assignment with rules:
        /// Rule A: Don't remove jurors who have already graded
        /// Rule B: Distribute evenly (prioritize jurors with fewer activities)
        /// Rule C: Don't assign duplicates
        /// </summary>
        Task<bool> ImprovedRandomAssignAsync(int activityId, int juryPerSubmission);
    }
}
