using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.WeeklyQuizDto
{
    public class UpdateWeeklyQuizDto
    {
        [Required(ErrorMessage = "ID không được để trống")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Số tuần không được để trống")]
        [Range(1, 52, ErrorMessage = "Số tuần phải từ 1 đến 52")]
        public int WeekNumber { get; set; }

        [Required(ErrorMessage = "Năm không được để trống")]
        [Range(2020, 2100, ErrorMessage = "Năm không hợp lệ")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Danh sách câu hỏi không được để trống")]
        [MinLength(1, ErrorMessage = "Quiz phải có ít nhất 1 câu hỏi")]
        public List<CreateQuizQuestionDto> Questions { get; set; } = new();

        [Required(ErrorMessage = "Deadline không được để trống")]
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "Thời gian làm bài không được để trống")]
        [Range(1, 300, ErrorMessage = "Thời gian làm bài phải từ 1 đến 300 phút")]
        public int TimeLimitMinutes { get; set; }

        [Required(ErrorMessage = "Điểm tối đa không được để trống")]
        [Range(1, 1000, ErrorMessage = "Điểm tối đa phải từ 1 đến 1000")]
        public int MaxScore { get; set; }
    }
}

