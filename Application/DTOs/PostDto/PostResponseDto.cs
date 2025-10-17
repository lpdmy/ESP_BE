using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs
{
    public class PostResponseDto
    {
        public int Id { get; set; }
        [StringLength(500)]
        public string? Title { get; set; }
        [Required]
        public string Body { get; set; } = null!;
        public int UserId { get; set; }
        public string? UserFullName { get; set; }
        public int? ClassGroupId { get; set; }
        public int? ClubId { get; set; }
        public string? ClubName { get; set; }
        [StringLength(20)]
        public PostVisibility PrivacyLevel { get; set; }
        [StringLength(20)]
        public PostStatus Status { get; set; }
        [StringLength(20)]
        public string? CallToAction { get; set; }
        public bool IsDeleted { get; set; } = false;
        public List<string> Hashtags { get; set; } = new();
        public List<string> MentionUsernames { get; set; } = new();
        public List<string> Comments { get; set; } = new();
        public List<string> AttachmentUrls { get; set; } = new();
        public string AvatarUrl { get; set; } = null!;
        public int LikeCount { get; set; }
        public int ReportCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLikedByCurrentUser { get; set; } = false;
    }

}
