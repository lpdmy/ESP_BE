namespace EduShpere.Application.DTOs.ClassGroupDto;

public class ClassGroupDetailDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Grade { get; set; }
    public int? AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }
    public int CurrentStudentCount { get; set; }
    public HomeroomTeacherDto? HomeroomTeacher { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Lịch học của lớp (optional - backward compatible)
    /// </summary>
    public List<ClassGroupScheduleDto>? Schedules { get; set; }
}

public class HomeroomTeacherDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();
}
