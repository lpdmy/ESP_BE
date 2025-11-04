namespace EduShpere.Application.DTOs.ClassGroupDto;

public class AcademicYearDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsCurrent { get; set; }
}
