using System;
using System.Collections.Generic;

namespace EduShpere.Application.DTOs.WeeklyQuizDto
{
    public class QuizSubmissionResponseDto
    {
        public string Id { get; set; }
        public string QuizId { get; set; }
        public string? QuizTitle { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public List<AnswerSubmissionResponseDto> Answers { get; set; } = new();
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public double Percentage => MaxScore > 0 ? (double)Score / MaxScore * 100 : 0;
        public DateTime StartedAt { get; set; }
        public DateTime SubmittedAt { get; set; }
        public int TimeSpentSeconds { get; set; }
        public bool IsGraded { get; set; }
    }

    public class AnswerSubmissionResponseDto
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; }
        public int Points { get; set; }
        public bool IsCorrect { get; set; }
        public string? CorrectAnswer { get; set; } // Chỉ trả về sau khi submit
    }
}

