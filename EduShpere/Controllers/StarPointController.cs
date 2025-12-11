using EduShpere.Application.DTOs;
using EduShpere.Application;
using EduShpere.Application.DTOs.StarPointDto;
using EduShpere.Application.Services.StarPointService;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Middlewares;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShpere.Infrastructure.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;
using EduShpere.Infrastructure.Repositories.StarPoint;
using static EduShpere.Shared.Constants.ApiEndpoints;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class StarPointController : BaseController
    {
        private readonly IRewardRuleService _rewardRuleService;
        private readonly IRewardService _rewardService;
        private readonly IRewardRedemptionService _rewardRedeemService;
        private readonly IPointHistoryService _pointHistoryService;
        public StarPointController(IRewardRuleService rewardRuleService, IRewardService rewardService, IRewardRedemptionService rewardRedeemService, IPointHistoryService pointHistoryService)
        {
            _rewardRuleService = rewardRuleService;
            _rewardService = rewardService;
            _rewardRedeemService = rewardRedeemService;
            _pointHistoryService = pointHistoryService;
        }

        /// <summary>
        /// Lấy danh sách tất cả reward rules
        /// </summary>
        [HttpGet(ApiEndpoints.StarPoint.GetAllRules)]
        public async Task<IActionResult> GetAllRules()
        {
            try
            {
                var rules = await _rewardRuleService.GetAllAsync();
                return Ok(new { Data = rules, Message = "Lấy danh sách reward rules thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Data = (object)null, Message = $"Lỗi khi lấy reward rules: {ex.Message}" });
            }
        }

        /// <summary>
        /// Cập nhật điểm cho một action
        /// </summary>
        [HttpPut(ApiEndpoints.StarPoint.UpdateRulePoints)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRulePoints(RewardActionType actionType, [FromBody] UpdatePointsRequest request)
        {
            try
            {
                var updatedRule = await _rewardRuleService.UpdatePointsAsync(actionType, request.Points);
                return Ok(new { Data = updatedRule, Message = $"Cập nhật điểm cho {actionType} thành công" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Data = (object)null, Message = $"Không tìm thấy reward rule với action {actionType}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Data = (object)null, Message = $"Cập nhật thất bại: {ex.Message}" });
            }
        }

        [HttpGet(ApiEndpoints.StarPoint.GetAllRewards)]
        public async Task<IActionResult> GetAllRewards()
        {
            var rewards = await _rewardService.GetAllAsync();
            return Ok(new ResponseDto<List<RewardWithClaimedDto>>(rewards, "", 200));
        }

        [HttpGet(ApiEndpoints.StarPoint.GetRewardById)]
        public async Task<IActionResult> GetRewardById(int id)
        {
            var reward = await _rewardService.GetByIdAsync(id);
            if (reward == null) return NotFound();
            return Ok(MapToResponseDto(reward));
        }

        [HttpPost(ApiEndpoints.StarPoint.CreateReward)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateReward([FromBody] RewardDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var reward = new Reward
            {
                Name = dto.Name,
                PointCost = dto.PointCost,
                Stock = dto.Stock,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl
            };

            var created = await _rewardService.CreateAsync(reward);
            return CreatedAtAction(nameof(GetRewardById), new { id = created.Id }, MapToResponseDto(created));
        }

        [HttpPut(ApiEndpoints.StarPoint.UpdateReward)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateReward(int id, [FromBody] RewardDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var reward = new Reward
            {
                Name = dto.Name,
                PointCost = dto.PointCost,
                Stock = dto.Stock,
                Category = dto.Category,
                ImageUrl = dto.ImageUrl
            };

            var updated = await _rewardService.UpdateAsync(id, reward);
            if (updated == null) return NotFound();
            return Ok(MapToResponseDto(updated));
        }

        [HttpDelete(ApiEndpoints.StarPoint.DeleteReward)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReward(int id)
        {
            var deleted = await _rewardService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost(ApiEndpoints.StarPoint.RedeemReward)]
        public async Task<IActionResult> RedeemReward([FromBody] RewardRedemptionDTO dto)
        {
            try
            {
                var result = await _rewardRedeemService.RedeemRewardAsync(dto);
                return Ok(new ResponseDto<RewardRedemptionResponseDTO>(result, "", 200));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, ex.Message, 401));
            }
        }

        [HttpGet(ApiEndpoints.StarPoint.GetPointHistory)]
        public async Task<IActionResult> GetPointHistory()
        {
            var data = await _pointHistoryService.GetUserHistoryAsync();
            return Ok(new ResponseDto<List<PointHistoryDto>>(data, "", 200));
        }

        [HttpPost(ApiEndpoints.StarPoint.CreatePointHistory)]
        public async Task<IActionResult> Create([FromBody] CreatePointHistoryDto dto)
        {
            var result = await _pointHistoryService.CreateHistoryAsync(dto);
            return Ok(new ResponseDto<PointHistoryDto>(result, "", 200));
        }

        [HttpGet(ApiEndpoints.StarPoint.GetCurrentUserPoints)]
        public async Task<ActionResult<UserPointsDto>> GetCurrentUserPoints(int userId)
        {
            var result = await _pointHistoryService.GetCurrentUserPoints(userId);
            return Ok(new ResponseDto<UserPointsDto>(result, "", 200));
        }

        [HttpGet(StarPoint.GetAllRedemptionsAdmin)]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] RedeemQueryParameters query)
        {
            var result = await _rewardRedeemService.GetAdminListAsync(query);
            return Ok(result);
        }

        [HttpGet(StarPoint.GetMyRedemptions)]
        public async Task<IActionResult> GetMyRedemptions([FromQuery] RedeemQueryParameters query)
        {
            var result = await _rewardRedeemService.GetUserListAsync(query);
            return Ok(result);
        }

        [HttpPost(StarPoint.PickupRedemption)]
        public async Task<IActionResult> Pickup(int id)
        {
            var result = await _rewardRedeemService.MarkAsPickedUpAsync(id);
            if (!result) return NotFound();
            return Ok();
        }

        private RewardResponseDto MapToResponseDto(Reward reward)
        {
            return new RewardResponseDto
            {
                Id = reward.Id,
                Name = reward.Name,
                PointCost = reward.PointCost,
                Stock = reward.Stock,
                Category = reward.Category,
                ImageUrl = reward.ImageUrl,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = reward.UpdatedAt,
                CreatedBy = reward.CreatedBy,
                UpdatedBy = reward.UpdatedBy
            };
        }
    }
}
