using EduShpere.Domain.Models;
using EduSphere.Domain.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure
{
    public class AppMongoDbContext
    {
        private readonly IMongoDatabase _database;

        public AppMongoDbContext(IConfiguration config)
        {
            var client = new MongoClient(config["MongoSettings:ConnectionString"]);
            _database = client.GetDatabase(config["MongoSettings:DatabaseName"]);
        }

        public IMongoCollection<Notification> Notifications
            => _database.GetCollection<Notification>("Notifications");

        public IMongoCollection<ChatMessage> ChatMessages
            => _database.GetCollection<ChatMessage>("ChatMessages");

        public IMongoCollection<ChatRoom> ChatRooms
            => _database.GetCollection<ChatRoom>("ChatRooms");
    }
}
