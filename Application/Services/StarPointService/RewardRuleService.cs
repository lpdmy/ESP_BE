using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories.StarPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    public class RewardRuleService : IRewardRuleService
    {
        private readonly IRewardRuleRepository _repository;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUserService;

        public RewardRuleService(IRewardRuleRepository repository, IAuditService auditService, ICurrentUserService currentUserService)
        {
            _repository = repository;
            _auditService = auditService;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<RewardRule>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<RewardRule> UpdatePointsAsync(RewardActionType actionType, int points)
        {
            var userId = _currentUserService.GetCurrentUserId() ?? 0;
            var updatedRule = await _repository.UpdatePointsAsync(actionType, points, userId);
            return updatedRule;
        }
    }
}
