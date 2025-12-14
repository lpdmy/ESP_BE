using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// DTO cho validation error khi import
/// </summary>
public class ImportErrorDto
{
    public int Row { get; set; }
    public string Message { get; set; } = null!;
    public string? Field { get; set; }
}

/// <summary>
/// DTO cho response khi import activities
/// </summary>
public class ImportActivityResponseDto
{
    public bool Valid { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = new List<ImportErrorDto>();
    public List<CreateActivityDto> Activities { get; set; } = new List<CreateActivityDto>();
}

/// <summary>
/// DTO cho bulk create activities
/// </summary>
public class BulkCreateActivitiesDto
{
    [Required(ErrorMessage = "Danh sách hoạt động không được để trống")]
    public List<CreateActivityDto> Activities { get; set; } = new List<CreateActivityDto>();
}

/// <summary>
/// DTO cho import activities từ file
/// </summary>
public class ImportActivityFileDto
{
    [Required(ErrorMessage = "File không được để trống")]
    public IFormFile File { get; set; } = null!;
}

