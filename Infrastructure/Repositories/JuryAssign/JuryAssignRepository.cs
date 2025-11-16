using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class JuryAssignRepository : BaseRepository<JuryAssignment>, IJuryAssignRepository
    {
        public JuryAssignRepository(EduShpereDbContext context) : base(context)
        {
        }
        public async Task<int> GetCountAsyncBySubmission(int submissionId)
        {
            return await _dbSet.CountAsync(ja => ja.SubmissionId == submissionId);
        }
        public int GetCountAsyncByUser(int userId)
        {
            return  _dbSet.Count(ja => ja.UserId == userId);
        }
        public async Task<List<JuryAssignment>> GetAllBySubmissionId(int id)
        {
            return await _dbSet
                .Where(ja => ja.SubmissionId == id)
                .ToListAsync();
        }
        public async Task<bool> IsExisting(int userId, int submissionId)
        {
            return await _dbSet.AnyAsync(ja => ja.UserId == userId && ja.SubmissionId == submissionId);
        }
        public async Task<List<JuryAssignment>> GetAllByActivityId(int activityId)
        {
            return await _dbSet
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.Activity)
                .Where(ja => ja.Submission.ActivityId == activityId)
                .ToListAsync();
        }
        public IQueryable <JuryAssignment> GetAllByActivityIdUserId(int activityId,int userId)
        {
            return _dbSet
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.User)
                .Where(ja => ja.Submission.ActivityId == activityId && ja.UserId == userId );
        }
        public IQueryable<JuryAssignment> GetAllByActivityIdUserIdNotGrading(int activityId, int userId)
        {
            return _dbSet
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.User)
                .Where(ja => ja.Submission.ActivityId == activityId && ja.UserId == userId && ja.ScoreTemp == null);
        }
        public async Task DeleteAllByActivityIdAsync(int activityId)
        {
            var assignments = await _context.JuryAssignment
                .Where(a => a.Submission.ActivityId == activityId)
                .ToListAsync();
            if (assignments.Count == 0)
                return;

            _context.JuryAssignment.RemoveRange(assignments);
            await _context.SaveChangesAsync();
        }


    }
}
