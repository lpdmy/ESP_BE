using EduShpere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.NotificationService
{
    public interface INotificationService
    {
        Task<List<Notification>> GetByUserAsync(int userId);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(string id);
    }
}
