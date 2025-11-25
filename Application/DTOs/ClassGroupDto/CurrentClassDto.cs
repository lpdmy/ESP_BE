namespace EduShpere.Application.DTOs.ClassGroupDto;

public class CurrentClassDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? Grade { get; set; }
    public AcademicYearDto? AcademicYear { get; set; }
    public HomeroomTeacherDto? HomeroomTeacher { get; set; }
    public int StudentCount { get; set; }
    public string UserRole { get; set; } = string.Empty; // "Student" or "Teacher"
    public DateTime? JoinedAt { get; set; } // For students
    public DateTime? AssignedAt { get; set; } // For teachers
    
    /// <summary>
    /// Lịch học của lớp (optional - backward compatible)
    /// </summary>
    public List<ClassGroupScheduleDto>? Schedules { get; set; }
}
