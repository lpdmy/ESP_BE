using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.StarPoint
{
    public interface IRewardRuleRepository
    {
        Task<IEnumerable<RewardRule>> GetAllAsync();
        Task<RewardRule?> GetByActionTypeAsync(RewardActionType actionType);
        Task<RewardRule> UpdatePointsAsync(RewardActionType actionType, int points, int updatedBy);
    }
}
