using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EduShpere.Domain.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string? Avatar { get; set; }
        public string Title { get; set; }
        public string? Link { get; set; }
        public bool Read { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<NotificationAction> Actions { get; set; } = new();
    }
    public class NotificationAction
    {
        public string Type { get; set; }
        public string Label { get; set; }
        public string Variant { get; set; }
    }
}
