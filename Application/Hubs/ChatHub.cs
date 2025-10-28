using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using EduShpere.Application.Services.ChatService;
using EduSphere.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace EduShpere.Hubs
{
        public class ChatHub : Hub
        {
            public async Task SendMessage(string roomId, int senderId, string content, string type = "text")
            {
                var message = new ChatMessage
                {
                    RoomId = roomId,
                    SenderId = senderId,
                    Content = content,
                    Type = type,
                    Timestamp = DateTime.UtcNow
                };

                // ✅ chỉ broadcast tới group, không lưu DB
                await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
            }

            public async Task JoinRoom(string roomId)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                Console.WriteLine($"✅ {Context.ConnectionId} joined room {roomId}");
            }

            public async Task LeaveRoom(string roomId)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                Console.WriteLine($"🚪 {Context.ConnectionId} left room {roomId}");
            }
        }
    }


