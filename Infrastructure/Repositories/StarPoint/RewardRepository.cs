using EduShpere.Domain.Models;
using EduShpere.Infrastructure.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.StarPoint
{
    public class RewardRepository : IRewardRepository
    {
        private readonly EduShpereDbContext _context;

        public RewardRepository(EduShpereDbContext context)
        {
            _context = context;
        }

        public async Task<List<RewardWithClaimedDto>> GetAllAsync()
        {
            var rewards = await _context.Rewards
                .Select(r => new RewardWithClaimedDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    PointCost = r.PointCost,
                    Stock = r.Stock,
                    Category = r.Category,
                    ImageUrl = r.ImageUrl,
                    Claimed = _context.RewardRedemptions
                                .Where(rr => rr.RewardId == r.Id)
                                .Sum(rr => rr.Quantity)
                })
                .ToListAsync();

            return rewards;
        }
        public async Task<Reward> GetByIdAsync(int id)
        {
            return await _context.Rewards.FindAsync(id);
        }

        public async Task<Reward> AddAsync(Reward reward)
        {
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();
            return reward;
        }

        public async Task<Reward> UpdateAsync(Reward reward)
        {
            _context.Rewards.Update(reward);
            await _context.SaveChangesAsync();
            return reward;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null) return false;
            _context.Rewards.Remove(reward);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
