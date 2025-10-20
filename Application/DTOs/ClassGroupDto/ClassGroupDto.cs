namespace EduShpere.Application.DTOs.ClassGroupDto;

public class ClassGroupDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Grade { get; set; }
    public int? AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }
    public int CurrentStudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
