using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Infrastructure.Repositories
{
    public class ModerationRepository : BaseRepository<ReportedContent>, IModerationRepository
    {
        public ModerationRepository(EduShpereDbContext dbContext) : base(dbContext)
        {
        }
        public IQueryable<ReportedContent> GetAllModerationIncluded()
        {
            return _dbSet
                .Include(rc => rc.Author)
                .Include(rc => rc.Reporter)
                .Include(rc => rc.Reviewer)
                .OrderByDescending(rc=>rc.ReportedAt);
        }
        public IQueryable<UserViolationStatModel> GetUserViolationStatsRaw()
        {
            return _dbSet
                .Include(rc => rc.Author)
                .Where(rc=>rc.Status=="Approved")
                .GroupBy(rc => new {
                    rc.AuthorId,
                    rc.Author.FirstName,
                    rc.Author.LastName
                })
                .Select(g => new UserViolationStatModel
                {
                    AuthorId = g.Key.AuthorId ?? 0,
                    AuthorName = g.Key.LastName + " " + g.Key.FirstName,
                    ViolationCount = g.Count(),
                    LatestViolation = g.Max(x => x.ReportedAt)
                }).OrderByDescending(g=>g.ViolationCount);
        }

        
    }
}
