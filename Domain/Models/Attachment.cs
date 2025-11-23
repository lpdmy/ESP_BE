using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Domain.Models;

public partial class Attachment
{
    [Key]
    public int Id { get; set; }

    public int? PostId { get; set; }

    public int? CommentId { get; set; }

    public int? SubmissionId { get; set; }

    [StringLength(1000)]
    public string? FileUrl { get; set; }

    [StringLength(255)]
    public string? FileName { get; set; }

    [StringLength(100)]
    public string? FileType { get; set; }

    [StringLength(100)]
    public string? Mime { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    [StringLength(30)]
    public string? ModerationStatus { get; set; }

    public string? AiFlagsJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    [ForeignKey("CommentId")]
    [InverseProperty("Attachments")]
    public virtual Comment? Comment { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Attachments")]
    public virtual Post? Post { get; set; }

    [ForeignKey("SubmissionId")]
    [InverseProperty("Attachments")]
    public virtual Submission? Submission { get; set; }
}
