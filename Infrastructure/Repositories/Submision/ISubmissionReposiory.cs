using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface ISubmissionReposiory
    {
        public IQueryable<Submission> GetAllSubmissionsByActivityId(int ActivityId);
        IQueryable<Submission> GetAllSubmissionByUserByActivity(int userId, int activityId);
        IQueryable<Submission> GetAllSubmissionByUserByActivityNotGrading(int userId, int activityId);
    }
}
