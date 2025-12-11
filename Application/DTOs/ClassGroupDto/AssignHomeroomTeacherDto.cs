namespace EduShpere.Application.DTOs.ClassGroupDto;

public class AssignHomeroomTeacherDto
{
    public string Email { get; set; } = string.Empty;
}

public class AssignHomeroomTeacherResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CurrentClassName { get; set; }
    public int? CurrentClassId { get; set; }
}
