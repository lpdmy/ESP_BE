using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface ISubmissionReposiory : IRepository<Submission>
    {
        public IQueryable<Submission> GetAllSubmissionsByActivityId(int ActivityId);
        public IQueryable<Submission> GetAllSubmissionsByActivityIdAnonymous(int ActivityId);
        IQueryable<Submission> GetAllSubmissionByUserByActivity(int userId, int activityId);
        IQueryable<Submission> GetAllSubmissionByUserByActivityNotGrading(int userId, int activityId);
        IQueryable<Submission> GetAllSubmissionByUserByActivityGrading(int userId, int activityId);
        Task<List<Submission>> GetRankByActivityId(int id);
        List<Submission> FilterCompletedSubmissions(
    List<Submission> submissions,
    Dictionary<int, int> requiredJuryDict);
        IQueryable<Submission> GetAllSubmissionByUser(int UserId);
        Task<Submission> GetSubmissionById(int id);
        IQueryable<Submission> GetQueryable();
    }
}
