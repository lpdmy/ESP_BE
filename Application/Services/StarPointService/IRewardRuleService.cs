using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    public interface IRewardRuleService
    {
        Task<IEnumerable<RewardRule>> GetAllAsync();
        Task<RewardRule> UpdatePointsAsync(RewardActionType actionType, int points);
    }
}
