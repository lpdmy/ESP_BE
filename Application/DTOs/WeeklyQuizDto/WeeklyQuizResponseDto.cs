using System;
using System.Collections.Generic;

namespace EduShpere.Application.DTOs.WeeklyQuizDto
{
    public class WeeklyQuizResponseDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public List<QuizQuestionResponseDto> Questions { get; set; } = new();
        public DateTime Deadline { get; set; }
        public int TimeLimitMinutes { get; set; }
        public int MaxScore { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsExpired => DateTime.UtcNow > Deadline;
        public bool CanTake => DateTime.UtcNow <= Deadline;
    }

    public class QuizQuestionResponseDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public List<AnswerOptionResponseDto>? Options { get; set; }
        public string? CorrectAnswer { get; set; } // Chỉ trả về cho teacher/admin
        public int Points { get; set; }
        public int Order { get; set; }
    }

    public class AnswerOptionResponseDto
    {
        public int Index { get; set; }
        public string Text { get; set; }
    }
}

