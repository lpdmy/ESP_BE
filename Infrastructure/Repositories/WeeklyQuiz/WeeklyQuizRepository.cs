using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.WeeklyQuizRepo
{
    public class WeeklyQuizRepository : IWeeklyQuizRepository
    {
        private readonly IMongoCollection<WeeklyQuiz> _quizzes;

        public WeeklyQuizRepository(AppMongoDbContext context)
        {
            _quizzes = context.WeeklyQuizzes;
        }

        public async Task<WeeklyQuiz> AddAsync(WeeklyQuiz quiz)
        {
            quiz.CreatedAt = DateTime.UtcNow;
            quiz.UpdatedAt = DateTime.UtcNow;
            await _quizzes.InsertOneAsync(quiz);
            return quiz;
        }

        public async Task<WeeklyQuiz?> GetByIdAsync(string id)
        {
            return await _quizzes
                .Find(q => q.Id == id && q.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<List<WeeklyQuiz>> GetByWeekAndYearAsync(int weekNumber, int year)
        {
            return await _quizzes
                .Find(q => q.WeekNumber == weekNumber && q.Year == year && q.IsActive)
                .SortByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<WeeklyQuiz>> GetAllActiveAsync()
        {
            return await _quizzes
                .Find(q => q.IsActive)
                .SortByDescending(q => q.Year)
                .ThenByDescending(q => q.WeekNumber)
                .ToListAsync();
        }

        public async Task<List<WeeklyQuiz>> GetByTeacherAsync(int teacherId)
        {
            return await _quizzes
                .Find(q => q.CreatedByUserId == teacherId && q.IsActive)
                .SortByDescending(q => q.Year)
                .ThenByDescending(q => q.WeekNumber)
                .ToListAsync();
        }

        public async Task<WeeklyQuiz?> UpdateAsync(string id, WeeklyQuiz quiz)
        {
            quiz.UpdatedAt = DateTime.UtcNow;
            var result = await _quizzes.ReplaceOneAsync(
                q => q.Id == id && q.IsActive,
                quiz
            );
            return result.ModifiedCount > 0 ? quiz : null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<WeeklyQuiz>.Update
                .Set(q => q.IsActive, false)
                .Set(q => q.UpdatedAt, DateTime.UtcNow);
            
            var result = await _quizzes.UpdateOneAsync(
                q => q.Id == id,
                update
            );
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ExistsAsync(int weekNumber, int year, string? excludeId = null)
        {
            var filter = Builders<WeeklyQuiz>.Filter.And(
                Builders<WeeklyQuiz>.Filter.Eq(q => q.WeekNumber, weekNumber),
                Builders<WeeklyQuiz>.Filter.Eq(q => q.Year, year),
                Builders<WeeklyQuiz>.Filter.Eq(q => q.IsActive, true)
            );

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter = Builders<WeeklyQuiz>.Filter.And(
                    filter,
                    Builders<WeeklyQuiz>.Filter.Ne(q => q.Id, excludeId)
                );
            }

            var count = await _quizzes.CountDocumentsAsync(filter);
            return count > 0;
        }
    }
}

