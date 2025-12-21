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
        private static IMongoClient? _sharedClient; // Shared client để connection pooling

        public AppMongoDbContext(IConfiguration config)
        {
            var connectionString = config["MongoSettings:ConnectionString"];
            
            // Sử dụng shared client để tận dụng connection pooling
            if (_sharedClient == null)
            {
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                // Cấu hình connection pool
                settings.MaxConnectionPoolSize = 100;
                settings.MinConnectionPoolSize = 10;
                settings.ConnectTimeout = TimeSpan.FromSeconds(30);
                settings.SocketTimeout = TimeSpan.FromSeconds(30);
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
                // Retry configuration
                settings.RetryWrites = true;
                settings.RetryReads = true;
                
                _sharedClient = new MongoClient(settings);
            }
            
            _database = _sharedClient.GetDatabase(config["MongoSettings:DatabaseName"]);
        }

        public IMongoCollection<Notification> Notifications
            => _database.GetCollection<Notification>("Notifications");

        public IMongoCollection<ChatMessage> ChatMessages
            => _database.GetCollection<ChatMessage>("ChatMessages");

        public IMongoCollection<ChatRoom> ChatRooms
            => _database.GetCollection<ChatRoom>("ChatRooms");

        public IMongoCollection<WeeklyQuiz> WeeklyQuizzes
            => _database.GetCollection<WeeklyQuiz>("WeeklyQuizzes");

        public IMongoCollection<QuizSubmission> QuizSubmissions
            => _database.GetCollection<QuizSubmission>("QuizSubmissions");
    }
}
