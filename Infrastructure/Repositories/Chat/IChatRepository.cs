using EduSphere.Domain.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.Chat
{
    public interface IChatRepository
    {
        Task<List<ChatRoom>> GetRoomsByUserId(int userId);
        Task<List<ChatMessage>> GetMessagesByRoomId(string roomId, int limit);
        Task<ChatMessage> AddMessage(ChatMessage message);
        Task<ChatRoom> CreateRoom(ChatRoom room);
        Task<ChatRoom> GetRoom(FilterDefinition<ChatRoom> filter);
        Task<ChatRoom> GetRoomById(string id);
        Task<ChatMessage?> GetLastMessageByRoomId(string roomId);
        Task MarkMessagesAsRead(string roomId, int userId);
    }
}
