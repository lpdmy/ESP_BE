using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EduShpere.Domain.Enum;

namespace EduShpere.Domain.Models;

/// <summary>
/// Template cho các hoạt động, lưu trữ các mẫu có sẵn với pre-fill data và checklist
/// </summary>
public class ActivityTemplate : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Loại hoạt động (SeminarWorkshop, CreativeContest, SportsFestival)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string SubType { get; set; } = null!;

    /// <summary>
    /// JSON string chứa pre-fill data (title, description, location, organizer, etc.)
    /// </summary>
    public string? PrefillData { get; set; }

    /// <summary>
    /// JSON string chứa checklist công việc (array of strings)
    /// </summary>
    public string? Checklist { get; set; }

    /// <summary>
    /// Có phải template mặc định của hệ thống không (không thể xóa)
    /// </summary>
    public bool IsSystemTemplate { get; set; } = false;

    /// <summary>
    /// Số lần sử dụng template này
    /// </summary>
    public int UsageCount { get; set; } = 0;
}

