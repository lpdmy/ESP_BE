using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.StarPoint
{
    public class RewardRuleRepository : IRewardRuleRepository
    {
        private readonly EduShpereDbContext _context;
        private readonly DbSet<RewardRule> _dbSet;

        public RewardRuleRepository(EduShpereDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<RewardRule>();
        }

        public async Task<IEnumerable<RewardRule>> GetAllAsync()
        {
            return await _dbSet
                .Where(r => r.IsActive)
                .OrderBy(r => r.ActionType)
                .ToListAsync();
        }

        public async Task<RewardRule?> GetByActionTypeAsync(RewardActionType actionType)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.ActionType == actionType && r.IsActive);
        }

        public async Task<RewardRule> UpdatePointsAsync(RewardActionType actionType, int points, int updatedBy)
        {
            var rule = await _dbSet
                .FirstOrDefaultAsync(r => r.ActionType == actionType && r.IsActive);

            if (rule == null)
                throw new KeyNotFoundException($"RewardRule with ActionType {actionType} not found.");

            rule.Points = points;
            rule.UpdatedAt = DateTime.UtcNow;
            rule.UpdatedBy = updatedBy;

            _dbSet.Update(rule);
            await _context.SaveChangesAsync();

            return rule;
        }
    }
}
