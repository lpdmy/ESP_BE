using System;

namespace EduShpere.Application.DTOs.TimetableDto
{
    public class TimetableDto
    {
        public int Id { get; set; }
        public int ClassGroupId { get; set; }
        public string? ClassGroupName { get; set; }
        public int DayOfWeek { get; set; } // 1=Monday, 7=Sunday
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? SubjectName { get; set; }
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


