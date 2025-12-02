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
        public async Task<JuryAssignment> GetDetailById(int id)
        {
            return await _dbSet.Include(p => p.Submission).ThenInclude(o=>o.Activity).FirstOrDefaultAsync(ja => ja.Id == id);
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
        public IQueryable<JuryAssignment> GetAllByActivityIdUserId(int activityId, int userId)
        {
            return _dbSet
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.Activity)
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.User)
                .Where(ja => ja.Submission.ActivityId == activityId && ja.UserId == userId);
        }
        public IQueryable<JuryAssignment> GetAllByActivityIdUserIdGrading(int activityId, int userId)
        {
            return _dbSet
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.Activity)
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.User)
                .Where(ja => ja.Submission.ActivityId == activityId && ja.UserId == userId && ja.ScoreTemp != null);
        }

        public IQueryable<JuryAssignment> GetAllByActivityIdUserIdNotGrading(int activityId, int userId)
        {
            return _dbSet
                .Include(ja => ja.Submission)
                .ThenInclude(Submission => Submission.Activity)
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
        public async Task GradeSubmission(string json,int assignmentId,string? comment,int totalScore)
        {
            var assignment = await _dbSet.FindAsync(assignmentId);
            if (assignment != null)
            {
                assignment.ScoreTemp = json;
                assignment.Comment = comment;
                assignment.TotalScore = totalScore;
                _dbSet.Update(assignment);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<int> NumberJuryRequired(int submissionId)
        {
           return await _dbSet.CountAsync(j => j.SubmissionId == submissionId);
        }
        public async Task<int> NumberJuryGrade(int submissionId)
        {
            return await _dbSet.CountAsync(j => j.SubmissionId == submissionId && j.ScoreTemp != null);
        }
        public async Task<Dictionary<int, int>> NumberJuryRequiredByActivity(int activityId)
        {
            return await _dbSet.Where(a => a.Submission.ActivityId == activityId)
                .GroupBy(a => a.SubmissionId)
                .ToDictionaryAsync(
                    g => g.Key,        
                    g => g.Count()
                );
        }
    }
}
