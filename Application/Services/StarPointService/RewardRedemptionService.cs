using EduShpere.Application.DTOs.StarPointDto;
using EduShpere.Application.Services.StarPointService;
using EduShpere.Application.Services;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories.StarPoint;

public class RewardRedemptionService : IRewardRedemptionService
{
    private readonly IRewardRedemptionRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPointHistoryService _pointHistoryService;

    public RewardRedemptionService(
        IRewardRedemptionRepository repository,
        ICurrentUserService currentUserService,
        IPointHistoryService pointHistoryService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _pointHistoryService = pointHistoryService;
    }

    public async Task<RewardRedemptionResponseDTO> RedeemRewardAsync(RewardRedemptionDTO dto)
    {
        var reward = await _repository.GetRewardByIdAsync(dto.RewardId);

        if (reward == null)
            throw new Exception("Phần thưởng không tồn tại.");

        if (reward.Stock < dto.Quantity)
            throw new Exception("Số lượng phần thưởng không đủ.");

        var userId = _currentUserService.GetCurrentUserId() ?? 0;
        int totalPoints = reward.PointCost * dto.Quantity;

        // Tạo bản ghi đổi thưởng
        var redemption = new RewardRedemption
        {
            RewardId = dto.RewardId,
            UserId = userId,
            Quantity = dto.Quantity,
            TotalPointsSpent = totalPoints,
            RedeemedAt = DateTime.Now,
            CreatedBy = userId,
            CreatedAt = DateTime.Now
        };

        await _repository.AddAsync(redemption);

        // Trừ stock phần thưởng
        reward.Stock -= dto.Quantity;

        // Ghi vào PointHistory
        await _pointHistoryService.CreateHistoryAsync(new CreatePointHistoryDto
        {
            Points = totalPoints,
            ActionType = PointActionType.Redeem,
            Description = $"Đổi {dto.Quantity} x {reward.Name}"
        });

        await _repository.SaveChangesAsync();

        return new RewardRedemptionResponseDTO
        {
            Id = redemption.Id,
            RewardId = redemption.RewardId,
            RewardName = reward.Name,
            UserId = redemption.UserId,
            Quantity = redemption.Quantity,
            TotalPointsSpent = redemption.TotalPointsSpent,
            RedeemedAt = redemption.RedeemedAt
        };
    }

    public async Task<PagedResult<RewardRedemptionAdminDTO>> GetAdminListAsync(RedeemQueryParameters query)
    {
        return await _repository.GetAllAsync(query);
    }
    public async Task<PagedResult<RewardRedemption>> GetUserListAsync(RedeemQueryParameters query)
    {
        var userId = _currentUserService.GetCurrentUserId() ?? 0;
        return await _repository.GetByUserIdAsync(userId, query);
    }

    public async Task<bool> MarkAsPickedUpAsync(int id)
    {
        var redemption = await _repository.GetByIdAsync(id);
        if (redemption == null) return false;

        if (redemption.Reward.TypeRequiresPickup())
        {
            redemption.Status = RedemptionStatus.PickedUp;
            await _repository.UpdateAsync(redemption);
        }
        return true;
    }
}
