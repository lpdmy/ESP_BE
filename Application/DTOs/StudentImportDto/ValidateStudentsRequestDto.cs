using System.ComponentModel.DataAnnotations;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.DTOs
{
    public class ValidateStudentsRequestDto
    {
        [Required(ErrorMessage = ErrorMessages.StudentImport.StudentsRequired)]
        public List<Dictionary<string, string>> Students { get; set; } = new();

        [Required(ErrorMessage = ErrorMessages.StudentImport.MappingRequired)]
        public Dictionary<string, string> Mapping { get; set; } = new();
    }

    public class ValidateStudentsResponseDto
    {
        public bool Valid { get; set; }
        public List<ValidationErrorDto> Errors { get; set; } = new();
        public int ValidCount { get; set; }
        public int InvalidCount { get; set; }
    }

    public class ValidationErrorDto
    {
        public int RowIndex { get; set; }
        public Dictionary<string, string> Row { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
