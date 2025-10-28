using EduShpere.Application.DTOs.ChatDto;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories.Chat;
using EduSphere.Domain.Models;
using MongoDB.Driver;

namespace EduShpere.Application.Services.ChatService
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public ChatService(IChatRepository repo, IUserService userService, INotificationService notificationService)
        {
            _repo = repo;
            _userService = userService;
            _notificationService = notificationService;
        }

        public Task<List<ChatMessage>> GetRoomMessages(string roomId, int limit = 50)
            => _repo.GetMessagesByRoomId(roomId, limit);

        public async Task<ChatMessage> SendMessage(ChatMessage message)
        {
            var mes = await _repo.AddMessage(message);
            var user = await _userService.GetUserByIdAsync(message.SenderId);
            var room = await _repo.GetRoomById(message.RoomId);
            await _notificationService.AddAsync(new Notification
            {
                Link = $"/chat/{message.RoomId}",
                Title = $"{user.LastName} đã gửi cho bạn một tin nhắn",
                CreatedAt = DateTime.Now,
                Avatar = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
                Read = false,
                Type = "message",
                UserId = room.ParticipantIds.FirstOrDefault(id => id != message.SenderId)
            });
            return mes;
        }
        public async Task<ChatRoomDto> CreateRoom(ChatRoom room)
        {
            if (room.ParticipantIds != null && room.ParticipantIds.Count == 2)
            {
                var filter = Builders<ChatRoom>.Filter.And(
                    Builders<ChatRoom>.Filter.Size(r => r.ParticipantIds, 2),
                    Builders<ChatRoom>.Filter.All(r => r.ParticipantIds, room.ParticipantIds)
                );

                var existingRoom = await _repo.GetRoom(filter);
                if (existingRoom != null)
                    return await MapToRoomDto(existingRoom); // ✅ Trả về phòng cũ
            }

            // ❗ Tạo mới
            await _repo.CreateRoom(room);
            return await MapToRoomDto(room);
        }

        public async Task<List<ChatRoomDto>> GetUserRoomsWithNames(int userId)
        {
            var rooms = await _repo.GetRoomsByUserId(userId);
            var allIds = rooms.SelectMany(r => r.ParticipantIds).Distinct().ToList();

            // 🔹 Lấy thông tin người dùng tuần tự để tránh DbContext bị conflict
            var users = new List<User>();
            foreach (var id in allIds)
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user != null)
                    users.Add(user);
            }

            var userDict = users
                .Where(u => u != null)
                .ToDictionary(
                    u => u.Id,
                    u => new
                    {
                        FullName = $"{u.FirstName} {u.LastName}",
                        u.AvatarUrl
                    }
                );

            // 🔹 Lấy tin nhắn cuối cùng tuần tự (tránh chạy song song)
            var lastMsgDict = new Dictionary<string, ChatMessage>();
            foreach (var r in rooms)
            {
                var lastMsg = await _repo.GetLastMessageByRoomId(r.Id);
                lastMsgDict[r.Id] = lastMsg;
            }

            // 🔹 Map kết quả
            var result = rooms.Select(r =>
            {
                var unreadCount = 0;
                var userKey = userId.ToString();

                if (r.UnreadCounts != null && r.UnreadCounts.ContainsKey(userKey))
                    unreadCount = r.UnreadCounts[userKey];

                return new ChatRoomDto
                {
                    Id = r.Id,
                    ParticipantIds = r.ParticipantIds,
                    ParticipantNames = r.ParticipantIds
                        .Select(id => userDict.ContainsKey(id) ? userDict[id].FullName : "Unknown")
                        .ToList(),
                    ParticipantAvatars = r.ParticipantIds
                                        .Select(id => userDict.ContainsKey(id) ? userDict[id].AvatarUrl : null)
                                        .ToList(),
                    LastMessage = lastMsgDict.GetValueOrDefault(r.Id)?.Content,
                    UpdatedAt = lastMsgDict.GetValueOrDefault(r.Id)?.Timestamp,
                    UnreadCount = unreadCount
                };
            })
            .OrderByDescending(r => r.UpdatedAt)
            .ToList();

            return result;
        }

        public async Task<ChatRoomDetailDto> GetRoomDetail(string roomId, int limit = 50)
        {
            var room = await _repo.GetRoomById(roomId);
            if (room == null) return null;

            var messages = await _repo.GetMessagesByRoomId(roomId, limit);

            // 🔹 Lấy thông tin user tuần tự
            var users = new List<User>();
            foreach (var id in room.ParticipantIds)
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user != null)
                    users.Add(user);
            }

            var userDict = users
                .ToDictionary(
                    u => u.Id,
                    u => new
                    {
                        FullName = $"{u.FirstName} {u.LastName}",
                        u.AvatarUrl
                    }
                );

            return new ChatRoomDetailDto
            {
                Id = room.Id,
                ParticipantIds = room.ParticipantIds,
                ParticipantNames = room.ParticipantIds
                    .Select(id => userDict.ContainsKey(id) ? userDict[id].FullName : "Unknown")
                    .ToList(),
                ParticipantAvatars = room.ParticipantIds
                    .Select(id => userDict.ContainsKey(id) ? userDict[id].AvatarUrl : null)
                    .ToList(),
                Messages = messages
            };
        }

        private async Task<ChatRoomDto> MapToRoomDto(ChatRoom room)
        {
            // 🔹 Lấy thông tin user tuần tự
            var users = new List<User>();
            foreach (var id in room.ParticipantIds)
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user != null)
                    users.Add(user);
            }

            var userDict = users
                .ToDictionary(
                    u => u.Id,
                    u => new
                    {
                        FullName = $"{u.FirstName} {u.LastName}",
                        u.AvatarUrl
                    }
                );

            return new ChatRoomDto
            {
                Id = room.Id,
                ParticipantIds = room.ParticipantIds,
                ParticipantNames = room.ParticipantIds
                    .Select(id => userDict.ContainsKey(id) ? userDict[id].FullName : "Unknown")
                    .ToList(),
                ParticipantAvatars = room.ParticipantIds
                    .Select(id => userDict.ContainsKey(id) ? userDict[id].AvatarUrl : null)
                    .ToList()
            };
        }

        public async Task MarkMessagesAsRead(string roomId, int userId)
        {
            await _repo.MarkMessagesAsRead(roomId, userId);
        }
    }
}
