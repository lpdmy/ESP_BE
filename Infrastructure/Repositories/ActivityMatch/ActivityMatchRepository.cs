using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ActivityMatchRepository : BaseRepository<ActivityMatch>, IActivityMatchRepository
    {
        public ActivityMatchRepository(EduShpereDbContext context) : base(context)
        {
        }

        public async Task<ActivityMatch?> GetByIdWithIncludesAsync(int id)
        {
            return await _dbSet
                .Where(m => m.Id == id && !m.IsDeleted)
                .Include(m => m.Activity)
                .Include(m => m.Sport)
                .Include(m => m.ClassGroup1)
                .Include(m => m.ClassGroup2)
                .Include(m => m.WinnerClassGroup)
                .Include(m => m.NextMatch)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ActivityMatch>> GetByActivityIdAsync(int activityId)
        {
            return await _dbSet
                .Where(m => m.ActivityId == activityId && !m.IsDeleted)
                .Include(m => m.Sport)
                .Include(m => m.ClassGroup1)
                .Include(m => m.ClassGroup2)
                .Include(m => m.WinnerClassGroup)
                .OrderBy(m => m.Round)
                .ThenBy(m => m.MatchNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityMatch>> GetByActivityAndSportAsync(int activityId, int sportId)
        {
            return await _dbSet
                .Where(m => m.ActivityId == activityId && m.SportId == sportId && !m.IsDeleted)
                .Include(m => m.ClassGroup1)
                .Include(m => m.ClassGroup2)
                .Include(m => m.WinnerClassGroup)
                .OrderBy(m => m.Round)
                .ThenBy(m => m.MatchNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityMatch>> GetByActivityAndSportAndGradeAsync(int activityId, int sportId, int? grade)
        {
            var query = _dbSet
                .Where(m => m.ActivityId == activityId && m.SportId == sportId && !m.IsDeleted);

            if (grade.HasValue)
            {
                query = query.Where(m => m.Grade == grade.Value);
            }

            return await query
                .Include(m => m.ClassGroup1)
                .Include(m => m.ClassGroup2)
                .Include(m => m.WinnerClassGroup)
                .OrderBy(m => m.Round)
                .ThenBy(m => m.MatchNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityMatch>> GetByRoundAsync(int activityId, int sportId, int round, int? grade = null)
        {
            var query = _dbSet
                .Where(m => m.ActivityId == activityId && m.SportId == sportId && m.Round == round && !m.IsDeleted);

            if (grade.HasValue)
            {
                query = query.Where(m => m.Grade == grade.Value);
            }

            return await query
                .Include(m => m.ClassGroup1)
                .Include(m => m.ClassGroup2)
                .Include(m => m.WinnerClassGroup)
                .OrderBy(m => m.MatchNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ActivityMatch>> GetByClassGroupAsync(int classGroupId)
        {
            return await _dbSet
                .Where(m => !m.IsDeleted && 
                           (m.ClassGroup1Id == classGroupId || 
                            m.ClassGroup2Id == classGroupId || 
                            m.WinnerClassGroupId == classGroupId))
                .Include(m => m.Activity)
                .Include(m => m.Sport)
                .Include(m => m.ClassGroup1)
                .Include(m => m.ClassGroup2)
                .OrderByDescending(m => m.MatchDate)
                .ToListAsync();
        }

        public async Task<int> CountMatchesByActivityAndSportAsync(int activityId, int sportId, int? grade = null)
        {
            var query = _dbSet
                .Where(m => m.ActivityId == activityId && m.SportId == sportId && !m.IsDeleted);

            if (grade.HasValue)
            {
                query = query.Where(m => m.Grade == grade.Value);
            }

            return await query.CountAsync();
        }

        public async Task<bool> HasMatchesForActivityAndSportAsync(int activityId, int sportId, int? grade = null)
        {
            var query = _dbSet
                .Where(m => m.ActivityId == activityId && m.SportId == sportId && !m.IsDeleted);

            if (grade.HasValue)
            {
                query = query.Where(m => m.Grade == grade.Value);
            }

            return await query.AnyAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBracketAsync(int activityId, int sportId, int? grade = null)
        {
            var query = _dbSet
                .Where(m => m.ActivityId == activityId && m.SportId == sportId && !m.IsDeleted);

            if (grade.HasValue)
            {
                query = query.Where(m => m.Grade == grade.Value);
            }

            var matches = await query.ToListAsync();
            foreach (var match in matches)
            {
                match.IsDeleted = true;
                match.UpdatedAt = DateTime.UtcNow;
            }

            if (matches.Any())
            {
                _dbSet.UpdateRange(matches);
                await _context.SaveChangesAsync();
            }
        }
    }
}

