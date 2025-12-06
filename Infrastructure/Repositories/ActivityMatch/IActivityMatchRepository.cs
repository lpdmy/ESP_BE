using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IActivityMatchRepository
    {
        Task<IEnumerable<ActivityMatch>> GetAllAsync();
        Task<ActivityMatch?> GetByIdAsync(int id);
        Task<ActivityMatch?> GetByIdWithIncludesAsync(int id);
        Task AddAsync(ActivityMatch entity);
        Task AddRangeAsync(IEnumerable<ActivityMatch> entities);
        Task UpdateAsync(ActivityMatch entity);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);

        // Query methods
        Task<IEnumerable<ActivityMatch>> GetByActivityIdAsync(int activityId);
        Task<IEnumerable<ActivityMatch>> GetByActivityAndSportAsync(int activityId, int sportId);
        Task<IEnumerable<ActivityMatch>> GetByActivityAndSportAndGradeAsync(int activityId, int sportId, int? grade);
        Task<IEnumerable<ActivityMatch>> GetByRoundAsync(int activityId, int sportId, int round, int? grade = null);
        Task<IEnumerable<ActivityMatch>> GetByClassGroupAsync(int classGroupId);
        Task<int> CountMatchesByActivityAndSportAsync(int activityId, int sportId, int? grade = null);
        Task<bool> HasMatchesForActivityAndSportAsync(int activityId, int sportId, int? grade = null);
        Task DeleteBracketAsync(int activityId, int sportId, int? grade = null);
    }
}

