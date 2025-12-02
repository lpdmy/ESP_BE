using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application.DTOs.TimetableDto
{
    public class TimetableImportDto
    {
        [Required(ErrorMessage = "File là bắt buộc")]
        public IFormFile File { get; set; } = null!;
        
        public int? ClassGroupId { get; set; } // Optional: Nếu import cho 1 lớp cụ thể
        
        [Required(ErrorMessage = "AcademicYearId là bắt buộc")]
        public int AcademicYearId { get; set; }
        
        public bool ReplaceExisting { get; set; } = true; // Xóa dữ liệu cũ trước khi import
    }
    
    public class TimetableImportResultDto
    {
        public int ImportedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
}


