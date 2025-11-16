using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class SubmissionRepository : BaseRepository<Submission>, ISubmissionReposiory
    {
        public SubmissionRepository(EduShpereDbContext context) : base(context)
        {
        }
        public IQueryable<Submission> GetAllSubmissionsByActivityId(int ActivityId)
        {
            return _dbSet.Where(s => s.ActivityId == ActivityId)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s=>s.JuryAssignments);
        }
        public IQueryable<Submission>GetAllSubmissionByUserByActivity(int userId,int activityId)
        {
            return _dbSet.Where(s => s.UserId == userId && s.ActivityId==activityId)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s => s.JuryAssignments);
        }

        public IQueryable<Submission> GetAllSubmissionByUserByActivityNotGrading(int userId, int activityId)
        {
            return _dbSet.Where(s => s.UserId == userId && s.ActivityId == activityId).Where(p=>p.Score == null)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s => s.JuryAssignments);
        }
    }
}
