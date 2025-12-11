using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;

namespace EduShpere.Infrastructure.Repositories
{
    public interface IModerationRepository
    {
        Task<IEnumerable<ReportedContent>> GetAllAsync();
        Task<ReportedContent?> GetByIdAsync(int id);
        Task AddAsync(ReportedContent entity);
        Task AddRangeAsync(IEnumerable<ReportedContent> entities);
        Task UpdateAsync(ReportedContent entity);
        Task DeleteAsync(int id);
        IQueryable<ReportedContent> GetAllModerationIncluded();
        public IQueryable<UserViolationStatModel> GetUserViolationStatsRaw();

    }
}
