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
    }
}
