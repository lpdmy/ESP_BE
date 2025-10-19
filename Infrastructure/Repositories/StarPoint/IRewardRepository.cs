using EduShpere.Domain.Models;
using EduShpere.Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.StarPoint
{
    public interface IRewardRepository
    {
        Task<List<RewardWithClaimedDto>> GetAllAsync();
        Task<Reward> GetByIdAsync(int id);
        Task<Reward> AddAsync(Reward reward);
        Task<Reward> UpdateAsync(Reward reward);
        Task<bool> DeleteAsync(int id);
    }
}
