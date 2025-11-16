using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IJuryAssignRepository
    {
        Task<IEnumerable<JuryAssignment>> GetAllAsync();
        Task<JuryAssignment?> GetByIdAsync(int id);
        Task AddAsync(JuryAssignment entity);
        Task AddRangeAsync(IEnumerable<JuryAssignment> entities);
        Task UpdateAsync(JuryAssignment entity);
        Task DeleteAsync(int id);
        Task<int> GetCountAsyncBySubmission(int submissionId);
        Task<bool> IsExisting(int userId, int submissionId);
        Task<List<JuryAssignment>> GetAllBySubmissionId(int id);
        int GetCountAsyncByUser(int userId);
        Task<List<JuryAssignment>> GetAllByActivityId(int activityId);
        Task DeleteAllByActivityIdAsync(int activityId);
        IQueryable<JuryAssignment> GetAllByActivityIdUserId(int activityId, int userId);
        IQueryable<JuryAssignment> GetAllByActivityIdUserIdNotGrading(int activityId, int userId);
    }
}
