using EduShpere.Domain.Models;
using EduShpere.Infrastructure.DTOs;
using EduShpere.Infrastructure.Repositories.StarPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    public class RewardService : IRewardService
    {
        private readonly IRewardRepository _repository;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public RewardService(IRewardRepository repository, IAuditService auditService, ICurrentUserService currentUserService)
        {
            _repository = repository;
            _auditService = auditService;
            _currentUserService = currentUserService;
        }

        public async Task<List<RewardWithClaimedDto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Reward> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Reward> CreateAsync(Reward reward)
        {
            reward.CreatedAt = DateTime.Now;
            reward.CreatedBy = _currentUserService.GetCurrentUserId() ?? 0;
            return await _repository.AddAsync(reward);
        }

        public async Task<Reward> UpdateAsync(int id, Reward reward)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = reward.Name;
            existing.PointCost = reward.PointCost;
            existing.Stock = reward.Stock;
            existing.Category = reward.Category;
            existing.ImageUrl = reward.ImageUrl;
            existing.UpdatedAt = DateTime.Now;
            // Lấy user hiện tại thay vì dùng giá trị từ payload
            existing.UpdatedBy = _currentUserService.GetCurrentUserId() ?? existing.UpdatedBy;

            return await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }
    }
}
