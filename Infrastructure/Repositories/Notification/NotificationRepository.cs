using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories.Notifications;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationRepository(AppMongoDbContext context)
        {
            _notifications = context.Notifications;
        }

        public async Task AddAsync(Notification notification)
        {
            await _notifications.InsertOneAsync(notification);
        }

        public async Task<List<Notification>> GetByUserAsync(int userId)
        {
            return await _notifications
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(string id)
        {
            await _notifications.UpdateOneAsync(
                Builders<Notification>.Filter.Eq(n => n.Id, id),
                Builders<Notification>.Update.Set(n => n.Read, true)
            );
        }
    }
}
