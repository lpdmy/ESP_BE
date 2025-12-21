using EduShpere.Domain.Enum;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.StarPointService
{
    /// <summary>
    /// Service trung tâm để cộng điểm dựa trên RewardRule khi user thực hiện các hành động
    /// (đăng nhập, tạo bài viết, bình luận, nhận lượt thích, tham gia CLB, ...)
    /// </summary>
    public interface IUserActionRewardService
    {
        /// <summary>
        /// Cộng điểm cho user dựa trên RewardRule và gửi notification.
        /// Nếu rule không tồn tại hoặc bị inactive / points <= 0 thì không làm gì.
        /// </summary>
        Task AwardForActionAsync(int userId, RewardActionType actionType, string? descriptionOverride = null, string? link = null);
    }
}



