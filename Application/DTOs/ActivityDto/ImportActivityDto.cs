using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// DTO cho validation error khi import
/// Note: Được dùng bởi StudentImportService và các import service khác
/// </summary>
public class ImportErrorDto
{
    public int Row { get; set; }
    public string Message { get; set; } = null!;
    public string? Field { get; set; }
}

// Note: ImportActivityResponseDto, BulkCreateActivitiesDto, ImportActivityFileDto đã được xóa
// vì tính năng Import Activity đã bị loại bỏ

