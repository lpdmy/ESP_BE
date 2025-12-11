using System.ComponentModel.DataAnnotations;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.DTOs
{
    public class ImportStudentsRequestDto
    {
        [Required(ErrorMessage = ErrorMessages.StudentImport.StudentsRequired)]
        public List<Dictionary<string, string>> Students { get; set; } = new();

        [Required(ErrorMessage = ErrorMessages.StudentImport.MappingRequired)]
        public Dictionary<string, string> Mapping { get; set; } = new();

        [Required(ErrorMessage = ErrorMessages.StudentImport.InvalidImportMode)]
        [RegularExpression("^(insert|upsert)$", ErrorMessage = ErrorMessages.StudentImport.InvalidImportMode)]
        public string ImportMode { get; set; } = "insert";

        [Required(ErrorMessage = ErrorMessages.StudentImport.HeadersRequired)]
        public List<string> Headers { get; set; } = new();
    }
}
