namespace EduShpere.Application.DTOs.ClassGroupDto;

public class ClassGroupStudentDto
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public DateTime? Birthdate { get; set; }
    public string? StudentCode { get; set; }
    public string? FullName => $"{FirstName} {LastName}".Trim();
}

public class AddStudentToClassDto
{
    public string Email { get; set; } = string.Empty;
}

public class AddStudentToClassResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CurrentClassName { get; set; }
    public int? CurrentClassId { get; set; }
}

public class CreateStudentAndAddToClassDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public DateTime? Birthdate { get; set; }
}
