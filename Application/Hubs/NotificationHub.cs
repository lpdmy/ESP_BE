using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EduShpere.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string userId, object notification)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }
        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Connected user: {userId}");
            return base.OnConnectedAsync();
        }
    }
}
