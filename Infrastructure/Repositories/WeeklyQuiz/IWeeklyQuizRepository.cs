using EduShpere.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.WeeklyQuizRepo
{
    public interface IWeeklyQuizRepository
    {
        Task<WeeklyQuiz> AddAsync(WeeklyQuiz quiz);
        Task<WeeklyQuiz?> GetByIdAsync(string id);
        Task<List<WeeklyQuiz>> GetByWeekAndYearAsync(int weekNumber, int year);
        Task<List<WeeklyQuiz>> GetAllActiveAsync();
        Task<List<WeeklyQuiz>> GetByTeacherAsync(int teacherId);
        Task<WeeklyQuiz?> UpdateAsync(string id, WeeklyQuiz quiz);
        Task<bool> DeleteAsync(string id);
        Task<bool> ExistsAsync(int weekNumber, int year, string? excludeId = null);
    }
}

