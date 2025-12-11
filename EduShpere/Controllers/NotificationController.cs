using EduShpere.Application;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Domain.Models;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextService _httpContextService;

        public NotificationController(INotificationService notificationService, IHttpContextService httpContextService)
        {
            _notificationService = notificationService;
            _httpContextService = httpContextService;
        }

        [HttpGet(ApiEndpoints.Notification.GetByUser)]
        public async Task<IActionResult> GetByUser()
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null) return Unauthorized();
            var notifications = await _notificationService.GetByUserAsync((int) userId);
            if (notifications == null || !notifications.Any())
                return NotFound(new { message = "Không có thông báo nào." });

            return Ok(notifications);
        }

        [HttpPost(ApiEndpoints.Notification.AddNotification)]
        public async Task<IActionResult> AddNotification([FromBody] Notification notification)
        {
            if (notification == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            await _notificationService.AddAsync(notification);
            return Ok(new { message = "Thêm thông báo thành công!" });
        }

        [HttpPut(ApiEndpoints.Notification.MarkAsRead)]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Thông báo đã được đánh dấu là đã đọc." });
        }

        /// <summary>
        /// Test endpoint — thêm 1 thông báo giả lập
        /// </summary>
        [HttpPost(ApiEndpoints.Notification.AddTestNotification)]
        public async Task<IActionResult> AddTestNotification()
        {
            var notification = new Notification
            {
                UserId = 24,
                Title = "🎉 Chào mừng bạn đến với EduSphere!",
                Type = "system",
                Link = "/chat",
                CreatedAt = DateTime.UtcNow
            };

            await _notificationService.AddAsync(notification);
            return Ok(new { message = "Test notification created!" });
        }
    }
}
