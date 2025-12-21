using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.WeeklyQuizDto
{
    public class SubmitQuizDto
    {
        [Required(ErrorMessage = "Quiz ID không được để trống")]
        public string QuizId { get; set; }

        [Required(ErrorMessage = "Danh sách câu trả lời không được để trống")]
        public List<AnswerSubmissionDto> Answers { get; set; } = new();

        [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
        public DateTime StartedAt { get; set; }

        [Required(ErrorMessage = "Thời gian làm bài không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Thời gian làm bài không hợp lệ")]
        public int TimeSpentSeconds { get; set; }
    }

    public class AnswerSubmissionDto
    {
        [Required(ErrorMessage = "Question ID không được để trống")]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Câu trả lời không được để trống")]
        public string Answer { get; set; }
    }
}

