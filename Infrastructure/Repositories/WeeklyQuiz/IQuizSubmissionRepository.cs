using EduShpere.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.WeeklyQuizRepo
{
    public interface IQuizSubmissionRepository
    {
        Task<QuizSubmission> AddAsync(QuizSubmission submission);
        Task<QuizSubmission?> GetByIdAsync(string id);
        Task<QuizSubmission?> GetByQuizAndStudentAsync(string quizId, int studentId);
        Task<List<QuizSubmission>> GetByQuizAsync(string quizId);
        Task<List<QuizSubmission>> GetByStudentAsync(int studentId);
        Task<List<QuizSubmission>> GetByWeekAndYearAsync(int weekNumber, int year);
        Task<bool> HasStudentSubmittedAsync(string quizId, int studentId);
    }
}

