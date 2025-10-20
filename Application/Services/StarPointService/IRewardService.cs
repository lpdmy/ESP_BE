using EduShpere.Domain.Models;
using EduShpere.Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    public interface IRewardService
    {
        Task<List<RewardWithClaimedDto>> GetAllAsync();
        Task<Reward> GetByIdAsync(int id);
        Task<Reward> CreateAsync(Reward reward);
        Task<Reward> UpdateAsync(int id, Reward reward);
        Task<bool> DeleteAsync(int id);
    }
}
