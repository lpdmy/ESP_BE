using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

/// <summary>
/// DTO cho ActivityTemplate response
/// </summary>
public class ActivityTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string SubType { get; set; } = null!;
    public Dictionary<string, object>? PrefillData { get; set; }
    public List<string>? Checklist { get; set; }
    public bool IsSystemTemplate { get; set; }
    public int UsageCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO cho tạo ActivityTemplate mới
/// </summary>
public class CreateActivityTemplateDto
{
    [Required(ErrorMessage = "Tên mẫu không được để trống")]
    [StringLength(500, ErrorMessage = "Tên mẫu không được vượt quá 500 ký tự")]
    public string Name { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Loại hoạt động không được để trống")]
    [StringLength(50, ErrorMessage = "Loại hoạt động không được vượt quá 50 ký tự")]
    public string SubType { get; set; } = null!;

    /// <summary>
    /// Pre-fill data dưới dạng dictionary (sẽ được serialize thành JSON)
    /// </summary>
    public Dictionary<string, object>? PrefillData { get; set; }

    /// <summary>
    /// Checklist công việc (sẽ được serialize thành JSON array)
    /// </summary>
    public List<string>? Checklist { get; set; }
}

/// <summary>
/// DTO cho cập nhật ActivityTemplate
/// </summary>
public class UpdateActivityTemplateDto
{
    [Required(ErrorMessage = "ID không được để trống")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên mẫu không được để trống")]
    [StringLength(500, ErrorMessage = "Tên mẫu không được vượt quá 500 ký tự")]
    public string Name { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Loại hoạt động không được để trống")]
    [StringLength(50, ErrorMessage = "Loại hoạt động không được vượt quá 50 ký tự")]
    public string SubType { get; set; } = null!;

    public Dictionary<string, object>? PrefillData { get; set; }
    public List<string>? Checklist { get; set; }
}

