using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories.StarPoint;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    /// <summary>
    /// Service trung tâm để cộng điểm và gửi notification khi user thực hiện các hành động thường nhật.
    /// </summary>
    public class UserActionRewardService : IUserActionRewardService
    {
        private readonly IRewardRuleRepository _rewardRuleRepository;
        private readonly IPointHistoryService _pointHistoryService;
        private readonly INotificationService _notificationService;
        private readonly EduShpereDbContext _context;

        public UserActionRewardService(
            IRewardRuleRepository rewardRuleRepository,
            IPointHistoryService pointHistoryService,
            INotificationService notificationService,
            EduShpereDbContext context)
        {
            _rewardRuleRepository = rewardRuleRepository;
            _pointHistoryService = pointHistoryService;
            _notificationService = notificationService;
            _context = context;
        }

        public async Task AwardForActionAsync(int userId, RewardActionType actionType, string? descriptionOverride = null, string? link = null)
        {
            // Lấy rule tương ứng trong bảng RewardRules
            var rule = await _rewardRuleRepository.GetByActionTypeAsync(actionType);
            if (rule == null || !rule.IsActive || rule.Points <= 0)
            {
                return; // Không có rule hoặc rule không hợp lệ -> không cộng điểm
            }

            // Business rule: Login chỉ tính 1 lần/ngày
            if (actionType == RewardActionType.Login)
            {
                var today = DateTime.UtcNow.Date;
                var baseDescription = !string.IsNullOrWhiteSpace(rule.Description)
                    ? rule.Description
                    : GetDefaultDescription(actionType);

                // Check xem user đã nhận điểm đăng nhập trong ngày hôm nay chưa
                var hasReceivedToday = await _context.Set<PointHistory>()
                    .AnyAsync(ph => ph.UserId == userId
                        && ph.ActionType == PointActionType.Earn
                        && ph.Description.Contains(baseDescription)
                        && ph.CreatedAt.Date == today);

                if (hasReceivedToday)
                {
                    return; // Đã nhận điểm đăng nhập hôm nay rồi -> không cộng lại
                }
            }

            // Mô tả để lưu vào PointHistory
            var description = !string.IsNullOrWhiteSpace(rule.Description)
                ? rule.Description
                : GetDefaultDescription(actionType);

            var finalDescription = !string.IsNullOrWhiteSpace(descriptionOverride)
                ? $"{description} - {descriptionOverride}"
                : description;

            // Cộng điểm (PointHistory là nguồn sự thật)
            await _pointHistoryService.AddPointsWithTransactionAsync(
                userId,
                rule.Points,
                finalDescription,
                PointActionType.Earn
            );

            // Gửi notification realtime
            try
            {
                await _notificationService.AddAsync(new Notification
                {
                    UserId = userId,
                    Type = "starpoint",
                    Title = $"Bạn đã nhận được {rule.Points} điểm: {description}",
                    Link = link,
                    CreatedAt = DateTime.UtcNow,
                    Read = false
                });
            }
            catch
            {
                // Không để lỗi notification làm fail việc cộng điểm
            }
        }

        private static string GetDefaultDescription(RewardActionType actionType)
        {
            return actionType switch
            {
                RewardActionType.Login => "Đăng nhập hệ thống",
                RewardActionType.CommentPost => "Bình luận bài viết",
                RewardActionType.ReceiveLike => "Bài viết nhận lượt thích",
                RewardActionType.CreatePost => "Tạo bài viết mới",
                RewardActionType.JoinClub => "Tham gia câu lạc bộ",
                _ => "Hành động tích cực"
            };
        }
    }
}



