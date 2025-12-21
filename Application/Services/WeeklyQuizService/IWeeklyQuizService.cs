using EduShpere.Application.DTOs.WeeklyQuizDto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.WeeklyQuizService
{
    public interface IWeeklyQuizService
    {
        Task<WeeklyQuizResponseDto> CreateAsync(CreateWeeklyQuizDto dto, int teacherId, string? teacherName = null);
        Task<WeeklyQuizResponseDto?> GetByIdAsync(string id, bool includeCorrectAnswers = false);
        Task<List<WeeklyQuizResponseDto>> GetByWeekAndYearAsync(int weekNumber, int year);
        Task<List<WeeklyQuizResponseDto>> GetAllActiveAsync();
        Task<List<WeeklyQuizResponseDto>> GetByTeacherAsync(int teacherId);
        Task<WeeklyQuizResponseDto?> UpdateAsync(UpdateWeeklyQuizDto dto, int teacherId);
        Task<bool> DeleteAsync(string id, int teacherId);
        Task<QuizSubmissionResponseDto> SubmitQuizAsync(SubmitQuizDto dto, int studentId, string? studentName = null);
        Task<QuizSubmissionResponseDto?> GetSubmissionAsync(string quizId, int studentId);
        Task<List<QuizSubmissionResponseDto>> GetSubmissionsByQuizAsync(string quizId);
        Task<List<QuizSubmissionResponseDto>> GetSubmissionsByStudentAsync(int studentId);
        Task<bool> HasStudentSubmittedAsync(string quizId, int studentId);
    }
}

