using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace EduShpere.Domain.Models
{
    /// <summary>
    /// Quiz Submission Model - Lưu trong MongoDB
    /// Lưu kết quả làm bài của học sinh
    /// </summary>
    public class QuizSubmission
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// ID của quiz (WeeklyQuiz.Id)
        /// </summary>
        [BsonRequired]
        public string QuizId { get; set; }

        /// <summary>
        /// ID học sinh làm bài
        /// </summary>
        [BsonRequired]
        public int StudentId { get; set; }

        /// <summary>
        /// Tên học sinh (để hiển thị)
        /// </summary>
        public string? StudentName { get; set; }

        /// <summary>
        /// WeekNumber và Year để query nhanh
        /// </summary>
        [BsonRequired]
        public int WeekNumber { get; set; }

        [BsonRequired]
        public int Year { get; set; }

        /// <summary>
        /// Danh sách câu trả lời
        /// </summary>
        [BsonRequired]
        public List<AnswerSubmission> Answers { get; set; } = new();

        /// <summary>
        /// Điểm số đạt được
        /// </summary>
        [BsonRequired]
        public int Score { get; set; }

        /// <summary>
        /// Điểm tối đa của quiz
        /// </summary>
        [BsonRequired]
        public int MaxScore { get; set; }

        /// <summary>
        /// Thời gian bắt đầu làm bài (UTC)
        /// </summary>
        [BsonRequired]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian nộp bài (UTC)
        /// </summary>
        [BsonRequired]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian làm bài (giây)
        /// </summary>
        [BsonRequired]
        public int TimeSpentSeconds { get; set; }

        /// <summary>
        /// Đã chấm điểm chưa (auto-chấm cho MultipleChoice và TrueFalse)
        /// </summary>
        public bool IsGraded { get; set; } = false;
    }

    /// <summary>
    /// Câu trả lời của học sinh cho một câu hỏi
    /// </summary>
    public class AnswerSubmission
    {
        /// <summary>
        /// ID câu hỏi (QuizQuestion.QuestionId)
        /// </summary>
        [BsonRequired]
        public int QuestionId { get; set; }

        /// <summary>
        /// Câu trả lời của học sinh
        /// - MultipleChoice: index của option (0-based) dạng string "0", "1", ...
        /// - TrueFalse: "true" hoặc "false"
        /// - ShortAnswer: text answer
        /// </summary>
        [BsonRequired]
        public string Answer { get; set; }

        /// <summary>
        /// Điểm đạt được cho câu này
        /// </summary>
        [BsonRequired]
        public int Points { get; set; }

        /// <summary>
        /// Đã đúng chưa
        /// </summary>
        [BsonRequired]
        public bool IsCorrect { get; set; }
    }
}

