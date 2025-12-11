using EduShpere.Application.DTOs.ChatDto;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
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
        private readonly IClassGroupRepository _classGroupRepository;
        private readonly IClubMemberRepository _clubMemberRepository;

        public ChatService(
            IChatRepository repo,
            IUserService userService,
            INotificationService notificationService,
            IClassGroupRepository classGroupRepository,
            IClubMemberRepository clubMemberRepository)
        {
            _repo = repo;
            _userService = userService;
            _notificationService = notificationService;
            _classGroupRepository = classGroupRepository;
            _clubMemberRepository = clubMemberRepository;
        }

        public Task<List<ChatMessage>> GetRoomMessages(string roomId, int limit = 50)
            => _repo.GetMessagesByRoomId(roomId, limit);

        public async Task<ChatMessage> SendMessage(ChatMessage message)
        {
            var mes = await _repo.AddMessage(message);
            var user = await _userService.GetUserByIdAsync(message.SenderId);
            var room = await _repo.GetRoomById(message.RoomId);
            if (room != null)
            {
                var targets = room.ParticipantIds.Where(id => id != message.SenderId).ToList();
                foreach (var targetId in targets)
                {
            await _notificationService.AddAsync(new Notification
            {
                Link = $"/chat/{message.RoomId}",
                        Title = $"{user.LastName} đã gửi tin nhắn trong phòng chat",
                CreatedAt = DateTime.Now,
                Avatar = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
                Read = false,
                Type = "message",
                        UserId = targetId
            });
                }
            }
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
            await EnsureClassGroupRoomForUser(userId);
            await EnsureClubRoomsForUser(userId);

            var rooms = await _repo.GetRoomsByUserId(userId);
            var allIds = rooms.SelectMany(r => r.ParticipantIds).Distinct().ToList();

            // 🔹 Lấy thông tin người dùng tuần tự để tránh DbContext concurrency
            var users = new List<User>();
            foreach (var id in allIds)
            {
                var u = await _userService.GetUserByIdAsync(id);
                if (u != null) users.Add(u);
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

            // 🔹 Lấy tin nhắn cuối cùng song song
            var lastMsgTasks = rooms.ToDictionary(r => r.Id, r => _repo.GetLastMessageByRoomId(r.Id));
            await Task.WhenAll(lastMsgTasks.Values);
            var lastMsgDict = lastMsgTasks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result);

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
                    UnreadCount = unreadCount,
                    Name = r.Name,
                    RoomType = r.RoomType,
                    ClassGroupId = r.ClassGroupId,
                    ClubId = r.ClubId
                };
            })
            .OrderByDescending(r => r.UpdatedAt)
            .ToList();

            return result;
        }

        private async Task EnsureClassGroupRoomForUser(int userId)
        {
            var currentClass = await _classGroupRepository.GetCurrentClassByUserIdAsync(userId);
            if (currentClass == null)
                return;

            // Lấy toàn bộ học sinh trong lớp từ repository (đảm bảo include đầy đủ)
            var students = await _classGroupRepository.GetStudentsInClassAsync(currentClass.Id);
            var participantIds = students.Select(s => s.Id).ToList();

            if (currentClass.TeacherId.HasValue)
            {
                participantIds.Add(currentClass.TeacherId.Value);
            }

            if (!participantIds.Contains(userId))
            {
                participantIds.Add(userId);
            }

            participantIds = participantIds.Distinct().ToList();

            var filter = Builders<ChatRoom>.Filter.And(
                Builders<ChatRoom>.Filter.Eq(r => r.RoomType, "class"),
                Builders<ChatRoom>.Filter.Eq(r => r.ClassGroupId, currentClass.Id)
            );

            var existingRoom = await _repo.GetRoom(filter);
            var roomName = currentClass.Name;
            if (currentClass.Grade.HasValue && !string.IsNullOrWhiteSpace(currentClass.Name))
            {
                roomName = $"{currentClass.Grade.Value}{currentClass.Name}";
            }
            else if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = $"Lớp {currentClass.Id}";
            }
            if (existingRoom == null)
            {
                var room = new ChatRoom
                {
                    Name = roomName,
                    RoomType = "class",
                    ClassGroupId = currentClass.Id,
                    ParticipantIds = participantIds
                };
                await _repo.CreateRoom(room);
            }
            else
            {
                var changed = false;
                foreach (var pid in participantIds)
                {
                    if (!existingRoom.ParticipantIds.Contains(pid))
                    {
                        existingRoom.ParticipantIds.Add(pid);
                        changed = true;
                    }
                }

                if (existingRoom.Name != roomName)
                {
                    existingRoom.Name = roomName;
                    changed = true;
                }

                if (changed)
                {
                    await _repo.UpdateRoomAsync(existingRoom);
                }
            }
        }

        private async Task EnsureClubRoomsForUser(int userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return;

            // Lấy membership kèm theo Club bằng method chuyên dụng
            var memberships = _clubMemberRepository.GetClubMemberByUser(user)
                .Where(cm => !cm.IsDeleted)
                .ToList();

            if (!memberships.Any())
                return;

            foreach (var membership in memberships)
            {
                var club = membership.Club;
                if (club == null) continue;

                // Lấy tất cả thành viên CLB từ DB
                var allMembers = (await _clubMemberRepository.GetAllAsync())
                    .Where(cm => cm.ClubId == club.Id && !cm.IsDeleted)
                    .Select(cm => cm.UserId)
                    .Distinct()
                    .ToList();

                if (!allMembers.Contains(userId))
                    allMembers.Add(userId);

                var filter = Builders<ChatRoom>.Filter.And(
                    Builders<ChatRoom>.Filter.Eq(r => r.RoomType, "club"),
                    Builders<ChatRoom>.Filter.Eq(r => r.ClubId, club.Id)
                );

                var existingRoom = await _repo.GetRoom(filter);
                if (existingRoom == null)
                {
                    var room = new ChatRoom
                    {
                        Name = club.Name ?? $"CLB {club.Id}",
                        RoomType = "club",
                        ClubId = club.Id,
                        ParticipantIds = allMembers
                    };
                    await _repo.CreateRoom(room);
                }
                else
                {
                    var changed = false;
                    foreach (var pid in allMembers)
                    {
                        if (!existingRoom.ParticipantIds.Contains(pid))
                        {
                            existingRoom.ParticipantIds.Add(pid);
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        await _repo.UpdateRoomAsync(existingRoom);
                    }
                }
            }
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
                Messages = messages,
                Name = room.Name,
                RoomType = room.RoomType,
                ClassGroupId = room.ClassGroupId,
                ClubId = room.ClubId
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
