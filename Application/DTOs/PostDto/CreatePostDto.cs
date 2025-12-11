using System.ComponentModel.DataAnnotations;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.PostDto
{
   public class CreatePostDto
     {
      [StringLength(500)]
      public string? Title { get; set; }

      [Required]
      public string Body { get; set; } = null!;

      public int? ClassGroupId { get; set; }

      public int? ClubId { get; set; }

      [Required]
      public PostVisibility PrivacyLevel { get; set; }

      [Required]
      public PostStatus Status { get; set; }

      public string? CallToAction { get; set; } 
       
      public List<string> Hashtags { get; set; } = new();

      public List<int> MentionUsernames { get; set; } = new();

      public List<PostAttachmentDto> AttachmentUrls { get; set; } = new();
    }
}
