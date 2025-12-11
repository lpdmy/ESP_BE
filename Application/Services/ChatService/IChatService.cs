using EduShpere.Application.DTOs.ChatDto;
using EduSphere.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.Services.ChatService
{
    public interface IChatService
    {
        Task<ChatMessage> SendMessage(ChatMessage message);
        Task<List<ChatMessage>> GetRoomMessages(string roomId, int limit = 50);
        Task<ChatRoomDto> CreateRoom(ChatRoom room);
        Task<List<ChatRoomDto>> GetUserRoomsWithNames(int userId);
        Task<ChatRoomDetailDto> GetRoomDetail(string roomId, int limit = 50);
        Task MarkMessagesAsRead(string roomId, int userId);
    }
}
