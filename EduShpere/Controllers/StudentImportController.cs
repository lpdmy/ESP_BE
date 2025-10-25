using EduShpere.Application;
using EduShpere.Application.DTOs;
using EduShpere.Application.Services;
using EduShpere.Middlewares;
using EduShpere.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduShpere.Controllers
{
    [CustomModelValidationFilter]
    public class StudentImportController : BaseController
    {
        private readonly IStudentImportService _studentImportService;

        public StudentImportController(IStudentImportService studentImportService)
        {
            _studentImportService = studentImportService;
        }

        [HttpPost("api/admin/students/import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportStudents([FromBody] ImportStudentsRequestDto request)
        {
            try
            {
                var result = await _studentImportService.ImportStudentsAsync(request);
                return Ok(new ResponseDto<ImportStudentsResponseDto>(result, "Import học sinh thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"{ErrorMessages.StudentImport.ImportFailed}: {ex.Message}", 400));
            }
        }

        [HttpPost("api/admin/students/validate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ValidateStudents([FromBody] ValidateStudentsRequestDto request)
        {
            try
            {
                var result = await _studentImportService.ValidateStudentsAsync(request);
                return Ok(new ResponseDto<ValidateStudentsResponseDto>(result, "Validation dữ liệu thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"{ErrorMessages.StudentImport.ValidationFailed}: {ex.Message}", 400));
            }
        }

        [HttpGet("api/admin/students/template")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadTemplate()
        {
            try
            {
                var templateBytes = await _studentImportService.GenerateTemplateAsync();
                return File(templateBytes, "text/csv", "student-import-template.csv");
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto<string>(null, $"{ErrorMessages.StudentImport.TemplateGenerationFailed}: {ex.Message}", 400));
            }
        }
    }
}
