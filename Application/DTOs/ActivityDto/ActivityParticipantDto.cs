using System;
using System.Text.Json.Serialization;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivityParticipantDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public int UserId { get; set; }
    public int? ClassGroupId { get; set; }
    public int? SportId { get; set; }
    public string? Status { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = null!;
    public string? UserFullName { get; set; }
    public string? UserAvatarUrl { get; set; }
    public string? ClassGroupName { get; set; }
    public int? Grade { get; set; }
    public Guid? GroupCode { get; set; }
    public bool IsLeader { get; set; }
    public string? RegistrationMetadata { get; set; }
    public string? SportName { get; set; }

    [JsonIgnore]
    public ActivityResponseDto? Activity { get; set; }
}
