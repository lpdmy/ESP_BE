using System.ComponentModel.DataAnnotations;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.DTOs.ClassGroupDto;

public class CreateClassGroupDto
{
    [StringLength(255, ErrorMessage = ErrorMessages.Validation.NameTooLong)]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = ErrorMessages.Validation.DescriptionTooLong)]
    public string? Description { get; set; }

    [Range(10, 12, ErrorMessage = "Khối học phải từ 10 đến 12")]
    public int? Grade { get; set; }

    public int? AcademicYearId { get; set; }
}
