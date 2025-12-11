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
    public class RewardRedemptionRepository : IRewardRedemptionRepository
    {
        private readonly EduShpereDbContext _context;

        public RewardRedemptionRepository(EduShpereDbContext context)
        {
            _context = context;
        }

        public async Task<RewardRedemption> AddAsync(RewardRedemption entity)
        {
            await _context.RewardRedemptions.AddAsync(entity);
            return entity;
        }

        public async Task<Reward> GetRewardByIdAsync(int rewardId)
        {
            return await _context.Rewards.FirstOrDefaultAsync(r => r.Id == rewardId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<RewardRedemptionAdminDTO>> GetAllAsync(RedeemQueryParameters query)
        {
            var queryable = _context.RewardRedemptions
                .Select(r => new RewardRedemptionAdminDTO
                {
                    Id = r.Id,
                    Category = r.Reward.Category,
                    RewardName = r.Reward.Name,
                    UserName = r.User.LastName + " " + r.User.LastName,
                    StudentNumber = r.User.StudentProfile.StudentNumber,
                    Quantity = r.Quantity,
                    TotalPointsSpent = r.TotalPointsSpent,
                    RedeemedAt = r.RedeemedAt,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                });

            // Status filter
            if (!string.IsNullOrEmpty(query.Status) && query.Status.ToLower() != "all")
            {
                if (Enum.TryParse<RedemptionStatus>(query.Status, out var statusEnum))
                {
                    queryable = queryable.Where(r => r.Status == statusEnum);
                }
            }

            // Category filter
            if (!string.IsNullOrEmpty(query.Category) && query.Category.ToLower() != "all")
            {
                if (Enum.TryParse<RewardCategory>(query.Category, out var categoryEnum))
                {
                    queryable = queryable.Where(r => r.Category == categoryEnum);
                }
            }

            // QueryString filter
            if (!string.IsNullOrEmpty(query.QueryString))
            {
                queryable = queryable.Where(r => r.UserName.ToLower().Contains(query.QueryString) || r.StudentNumber.ToLower().Contains(query.QueryString));
            }

            // Tổng số record
            var totalCount = await queryable.CountAsync();

            // Paging
            var items = await queryable
                .OrderByDescending(r => r.CreatedAt) // hoặc cột khác phù hợp
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<RewardRedemptionAdminDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<PagedResult<RewardRedemption>> GetByUserIdAsync(int userId, RedeemQueryParameters query)
        {
            var queryable = _context.RewardRedemptions
                .Where(r => r.UserId == userId)
                .Include(r => r.Reward)
                .AsQueryable();

            // Status filter
            if (!string.IsNullOrEmpty(query.Status) && query.Status.ToLower() != "all")
            {
                if (Enum.TryParse<RedemptionStatus>(query.Status, out var statusEnum))
                {
                    queryable = queryable.Where(r => r.Status == statusEnum);
                }
            }

            // Category filter
            if (!string.IsNullOrEmpty(query.Category) && query.Category.ToLower() != "all")
            {
                if (Enum.TryParse<RewardCategory>(query.Category, out var categoryEnum))
                {
                    queryable = queryable.Where(r => r.Reward.Category == categoryEnum);
                }
            }

            // Tổng số record
            var totalCount = await queryable.CountAsync();

            // Paging
            var items = await queryable
                .OrderByDescending(r => r.CreatedAt) 
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<RewardRedemption>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<RewardRedemption> GetByIdAsync(int id)
        {
            return await _context.RewardRedemptions
                .Include(r => r.User)
                .Include(r => r.Reward)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(RewardRedemption redemption)
        {
            _context.RewardRedemptions.Update(redemption);
            await _context.SaveChangesAsync();
        }

    }
    public class RedeemQueryParameters
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; } // optional filter
        public string? Category { get; set; } // optional filter
        public string? QueryString { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class RewardRedemptionAdminDTO
    {
        public int Id { get; set; }
        public string RewardName { get; set; }
        public RewardCategory Category { get; set; }
        public string UserName { get; set; }
        public string StudentNumber { get; set; }
        public int Quantity { get; set; }
        public int TotalPointsSpent { get; set; }
        public DateTime RedeemedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public RedemptionStatus Status { get; set; }
    }

}
