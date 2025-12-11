using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

[Index("ActivityId", Name = "IX_ActivityDetail_ActivityId", IsUnique = true)]
public class ActivityDetail : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ActivityId { get; set; }

    // For SportsFestival
    [StringLength(50)]
    public string? CompetitionType { get; set; } // Individual, Team, Mixed

    // For CreativeContest
    [StringLength(500)]
    public string? Theme { get; set; }

    [StringLength(200)]
    public string? Genre { get; set; } // Loại hình sáng tạo: Vẽ tranh, Sáng tác văn học, Nhiếp ảnh, Video...

    [StringLength(200)]
    public string? PaperSize { get; set; } // Kích thước / Độ dài: A4, A3, 500-1000 từ, Tối đa 5 trang...

    [StringLength(200)]
    public string? DrawingMedium { get; set; } // Chất liệu / Thể loại: Màu nước, Chì màu, Truyện ngắn, Thơ, Digital...

    [StringLength(200)]
    public string? TimeLimit { get; set; } // Thời gian làm bài: 90 phút, 2 giờ, Tự do...

    [StringLength(500)]
    public string? SubmissionFormat { get; set; } // Format nộp bài: File số (JPG, PNG, PDF, Word), Bản giấy, Cả hai...

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("ActivityId")]
    [InverseProperty("ActivityDetail")]
    public virtual Activity Activity { get; set; } = null!;
}

