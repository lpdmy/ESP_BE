using EduShpere.Application.DTOs.StarPointDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories.StarPoint;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    public interface IRewardRedemptionService
    {
        Task<RewardRedemptionResponseDTO> RedeemRewardAsync(RewardRedemptionDTO dto);
        Task<PagedResult<RewardRedemptionAdminDTO>> GetAdminListAsync(RedeemQueryParameters query);
        Task<PagedResult<RewardRedemption>> GetUserListAsync(RedeemQueryParameters query);
        Task<bool> MarkAsPickedUpAsync(int id);
    }
}