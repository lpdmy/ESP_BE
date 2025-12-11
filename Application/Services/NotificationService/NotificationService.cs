using EduShpere.Domain.Models;
using EduShpere.Hubs;
using EduShpere.Infrastructure.Repositories.Notifications;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(INotificationRepository repo, IHubContext<NotificationHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetByUserAsync(int userId)
            => await _repo.GetByUserAsync(userId);

        public async Task AddAsync(Notification notification)
        {
            await _repo.AddAsync(notification);

            await _hubContext.Clients.User(notification.UserId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Title,
                    notification.Type,
                    notification.CreatedAt
                });
        }

        public async Task MarkAsReadAsync(string id)
            => await _repo.MarkAsReadAsync(id);
    }
}
