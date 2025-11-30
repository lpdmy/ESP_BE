

using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IActivityParticipantRepository
    {
        Task<IEnumerable<ActivityParticipant>> GetAllAsync();
        Task<ActivityParticipant?> GetByIdAsync(int id);
        Task AddAsync(ActivityParticipant entity);
        Task AddRangeAsync(IEnumerable<ActivityParticipant> entities);
        Task UpdateAsync(ActivityParticipant entity);
        Task DeleteAsync(int id);
        Task<bool> IsAlreadyRegisteredAsync(int userId, int activityId, int? sportId = null);
        Task SoftDeleteAsync(int id);
        Task<int> CountNumberParticipantInActivity(int activityId);
        Task<IEnumerable<ActivityParticipant>> GetByActivityIdAndUserIdAsync(int activityId, int userId);
    }
}
