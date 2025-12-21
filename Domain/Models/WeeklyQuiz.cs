using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace EduShpere.Domain.Models
{
    /// <summary>
    /// Weekly Quiz Model - Lưu trong MongoDB
    /// Mỗi quiz gắn với weekNumber và year
    /// </summary>
    public class WeeklyQuiz
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// Tiêu đề quiz
        /// </summary>
        [BsonRequired]
        public string Title { get; set; }

        /// <summary>
        /// Mô tả quiz (optional)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Số tuần trong năm (1-52)
        /// </summary>
        [BsonRequired]
        public int WeekNumber { get; set; }

        /// <summary>
        /// Năm học (ví dụ: 2024, 2025)
        /// </summary>
        [BsonRequired]
        public int Year { get; set; }

        /// <summary>
        /// Danh sách câu hỏi
        /// </summary>
        [BsonRequired]
        public List<QuizQuestion> Questions { get; set; } = new();

        /// <summary>
        /// Thời gian deadline (UTC)
        /// </summary>
        [BsonRequired]
        public DateTime Deadline { get; set; }

        /// <summary>
        /// Thời gian làm bài (phút)
        /// </summary>
        [BsonRequired]
        public int TimeLimitMinutes { get; set; }

        /// <summary>
        /// Điểm tối đa
        /// </summary>
        [BsonRequired]
        public int MaxScore { get; set; }

        /// <summary>
        /// ID giáo viên tạo quiz
        /// </summary>
        [BsonRequired]
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Tên giáo viên tạo quiz (để hiển thị)
        /// </summary>
        public string? CreatedByUserName { get; set; }

        /// <summary>
        /// Trạng thái: true = active, false = deleted
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian cập nhật
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Câu hỏi trong quiz
    /// </summary>
    public class QuizQuestion
    {
        /// <summary>
        /// ID câu hỏi (unique trong quiz)
        /// </summary>
        [BsonRequired]
        public int QuestionId { get; set; }

        /// <summary>
        /// Nội dung câu hỏi
        /// </summary>
        [BsonRequired]
        public string QuestionText { get; set; }

        /// <summary>
        /// Loại câu hỏi: MultipleChoice, TrueFalse, ShortAnswer
        /// </summary>
        [BsonRequired]
        public string QuestionType { get; set; }

        /// <summary>
        /// Danh sách đáp án (cho MultipleChoice)
        /// </summary>
        public List<QuizAnswerOption>? Options { get; set; }

        /// <summary>
        /// Đáp án đúng (có thể là string hoặc int index)
        /// - MultipleChoice: index của option đúng (0-based)
        /// - TrueFalse: "true" hoặc "false"
        /// - ShortAnswer: đáp án chính xác (hoặc keywords)
        /// </summary>
        [BsonRequired]
        public string CorrectAnswer { get; set; }

        /// <summary>
        /// Điểm số cho câu hỏi này
        /// </summary>
        [BsonRequired]
        public int Points { get; set; } = 1;

        /// <summary>
        /// Thứ tự hiển thị
        /// </summary>
        [BsonRequired]
        public int Order { get; set; }
    }

    /// <summary>
    /// Đáp án cho câu hỏi MultipleChoice
    /// </summary>
    public class QuizAnswerOption
    {
        /// <summary>
        /// Index của option (0-based)
        /// </summary>
        [BsonRequired]
        public int Index { get; set; }

        /// <summary>
        /// Nội dung đáp án
        /// </summary>
        [BsonRequired]
        public string Text { get; set; }
    }
}

