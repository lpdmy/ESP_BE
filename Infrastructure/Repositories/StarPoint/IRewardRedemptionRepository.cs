using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.StarPoint
{
    public interface IRewardRedemptionRepository
    {
        Task<RewardRedemption> AddAsync(RewardRedemption entity);
        Task<Reward> GetRewardByIdAsync(int rewardId);
        Task SaveChangesAsync();
        Task<PagedResult<RewardRedemptionAdminDTO>> GetAllAsync(RedeemQueryParameters query);
        Task<PagedResult<RewardRedemption>> GetByUserIdAsync(int userId, RedeemQueryParameters query);
        Task<RewardRedemption> GetByIdAsync(int id);
        Task UpdateAsync(RewardRedemption redemption);
    }
}
