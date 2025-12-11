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
            // Include User and ClassGroupMembers for Admin/Student view (need Class info)
            // JuryAssignments included for efficient counting
            return _dbSet.Where(s => s.ActivityId == ActivityId && !s.IsDeleted)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers.Where(cgm => !cgm.IsDeleted))
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.JuryAssignments)
                .Include(s => s.Attachments.Where(a => !a.IsDeleted));
        }
        
        public IQueryable<Submission> GetAllSubmissionsByActivityIdAnonymous(int ActivityId)
        {
            // For anonymous mode: minimal includes, no user details
            return _dbSet.Where(s => s.ActivityId == ActivityId && !s.IsDeleted)
                .Include(s => s.JuryAssignments)
                .Include(s => s.Attachments.Where(a => !a.IsDeleted));
        }
        public IQueryable<Submission>GetAllSubmissionByUserByActivity(int userId,int activityId)
        {
            return _dbSet.Where(s => s.UserId == userId && s.ActivityId==activityId && !s.IsDeleted)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s => s.JuryAssignments)
                .ThenInclude(ja => ja.User);
        }

        public IQueryable<Submission> GetAllSubmissionByUserByActivityNotGrading(int userId, int activityId)
        {
            return _dbSet.Where(s => s.UserId == userId && s.ActivityId == activityId && !s.IsDeleted && s.Score == null)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s => s.JuryAssignments)
                .ThenInclude(ja => ja.User);
        }
        public IQueryable<Submission> GetAllSubmissionByUserByActivityGrading(int userId, int activityId)
        {
            return _dbSet.Where(s => s.UserId == userId && s.ActivityId == activityId && !s.IsDeleted && s.Score != null)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s => s.JuryAssignments)
                .ThenInclude(ja => ja.User);
        }
        public async Task<List<Submission>> GetRankByActivityId(int id)
        {
            return await _dbSet.Where(s => s.ActivityId == id && s.Score.HasValue && !s.IsDeleted)
                .OrderByDescending(s => s.Score )
                .Include(s => s.User)
                .ToListAsync();
        }
        public List<Submission> FilterCompletedSubmissions(
    List<Submission> submissions,
    Dictionary<int, int> requiredJuryDict)
        {
            return submissions
                .Where(s =>
                    requiredJuryDict.TryGetValue(s.Id, out var requiredCount) &&
                    s.JuryAssignments.Count(j => j.TotalScore.HasValue) == requiredCount
                )
                .ToList();
        }
        public IQueryable<Submission> GetAllSubmissionByUser(int UserId)
        {
            return _dbSet.Where(s => s.UserId == UserId && !s.IsDeleted)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s => s.JuryAssignments)
                .ThenInclude(ja => ja.User);
        }

        public Task<Submission> GetSubmissionById(int id)
        {
            return _dbSet.Where(s => !s.IsDeleted)
                .Include(s => s.User)
                .ThenInclude(u => u.ClassGroupMembers)
                .ThenInclude(cgm => cgm.ClassGroup)
                .Include(s => s.Activity)
                .Include(s => s.JuryAssignments)
                .ThenInclude(ja => ja.User)
                .Include(s => s.Attachments.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public IQueryable<Submission> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
    }
}
