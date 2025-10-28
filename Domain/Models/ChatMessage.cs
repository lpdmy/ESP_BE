using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace EduSphere.Domain.Models
{
    public class ChatMessage
    {
        [BsonId]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string RoomId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; }
        public string Type { get; set; } = "text"; // text, image, file, etc.
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public List<int> ReadBy { get; set; } = new();
    }

    public class ChatRoom
    {
        [BsonId]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Name { get; set; }
        public List<int> ParticipantIds { get; set; } = new();

        public Dictionary<string, int> UnreadCounts { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
