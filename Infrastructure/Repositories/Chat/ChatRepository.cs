using EduShpere.Infrastructure;
using EduSphere.Domain.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppMongoDbContext _context;
        public ChatRepository(AppMongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatRoom>> GetRoomsByUserId(int userId)
        {
            var filter = Builders<ChatRoom>.Filter.AnyEq(r => r.ParticipantIds, userId);
            return await _context.ChatRooms.Find(filter).ToListAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesByRoomId(string roomId, int limit = 50)
        {
            var filter = Builders<ChatMessage>.Filter.Eq(m => m.RoomId, roomId);
            return await _context.ChatMessages.Find(filter)
                .SortByDescending(m => m.Timestamp)
                .Limit(limit)
                .ToListAsync();
        }
        public async Task MarkMessagesAsRead(string roomId, int userId)
        {
            var update = Builders<ChatRoom>.Update.Set($"UnreadCounts.{userId}", 0);
            await _context.ChatRooms.UpdateOneAsync(r => r.Id == roomId, update);
        }

        public async Task<ChatMessage> AddMessage(ChatMessage message)
        {
            await _context.ChatMessages.InsertOneAsync(message);

            var room = await _context.ChatRooms.Find(r => r.Id == message.RoomId).FirstOrDefaultAsync();
            if (room != null)
            {
                foreach (var userId in room.ParticipantIds)
                {
                    if (userId != message.SenderId)
                    {
                        var key = userId.ToString();
                        if (room.UnreadCounts.ContainsKey(key))
                            room.UnreadCounts[key]++;
                        else
                            room.UnreadCounts[key] = 1;
                    }
                }

                var update = Builders<ChatRoom>.Update.Set(r => r.UnreadCounts, room.UnreadCounts);
                await _context.ChatRooms.UpdateOneAsync(r => r.Id == room.Id, update);
            }

            return message;
        }


        public async Task<ChatRoom> CreateRoom(ChatRoom room)
        {
            await _context.ChatRooms.InsertOneAsync(room);
            return room;
        }
        public async Task<ChatRoom> GetRoom(FilterDefinition<ChatRoom> filter)
        {
            var room = await _context.ChatRooms.Find(filter).FirstOrDefaultAsync();
            return room;
        }

        public async Task<ChatRoom> GetRoomById(string id)
        {
            return await _context.ChatRooms
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ChatMessage?> GetLastMessageByRoomId(string roomId)
        {
            var filter = Builders<ChatMessage>.Filter.Eq(m => m.RoomId, roomId);
            return await _context.ChatMessages
                .Find(filter)
                .SortByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateRoomAsync(ChatRoom room)
        {
            var update = Builders<ChatRoom>.Update
                .Set(r => r.ParticipantIds, room.ParticipantIds)
                .Set(r => r.Name, room.Name)
                .Set(r => r.RoomType, room.RoomType)
                .Set(r => r.ClassGroupId, room.ClassGroupId)
                .Set(r => r.ClubId, room.ClubId);

            await _context.ChatRooms.UpdateOneAsync(r => r.Id == room.Id, update);
        }
    }
}
