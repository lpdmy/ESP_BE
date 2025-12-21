using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduShpere.Infrastructure.Repositories.WeeklyQuizRepo
{
    public class QuizSubmissionRepository : IQuizSubmissionRepository
    {
        private readonly IMongoCollection<QuizSubmission> _submissions;

        public QuizSubmissionRepository(AppMongoDbContext context)
        {
            _submissions = context.QuizSubmissions;
        }

        public async Task<QuizSubmission> AddAsync(QuizSubmission submission)
        {
            await _submissions.InsertOneAsync(submission);
            return submission;
        }

        public async Task<QuizSubmission?> GetByIdAsync(string id)
        {
            return await _submissions
                .Find(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<QuizSubmission?> GetByQuizAndStudentAsync(string quizId, int studentId)
        {
            return await _submissions
                .Find(s => s.QuizId == quizId && s.StudentId == studentId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<QuizSubmission>> GetByQuizAsync(string quizId)
        {
            return await _submissions
                .Find(s => s.QuizId == quizId)
                .SortByDescending(s => s.Score)
                .ThenByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<QuizSubmission>> GetByStudentAsync(int studentId)
        {
            return await _submissions
                .Find(s => s.StudentId == studentId)
                .SortByDescending(s => s.Year)
                .ThenByDescending(s => s.WeekNumber)
                .ToListAsync();
        }

        public async Task<List<QuizSubmission>> GetByWeekAndYearAsync(int weekNumber, int year)
        {
            return await _submissions
                .Find(s => s.WeekNumber == weekNumber && s.Year == year)
                .SortByDescending(s => s.Score)
                .ToListAsync();
        }

        public async Task<bool> HasStudentSubmittedAsync(string quizId, int studentId)
        {
            var count = await _submissions.CountDocumentsAsync(
                s => s.QuizId == quizId && s.StudentId == studentId
            );
            return count > 0;
        }
    }
}

