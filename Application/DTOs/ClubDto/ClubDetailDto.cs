using System;
using System.Collections.Generic;

namespace EduShpere.Application.DTOs
{
    /// <summary>
    /// DTO cho Club Detail - không bao gồm Members để tối ưu performance
    /// Members sẽ được fetch riêng qua endpoint GetClubMembers
    /// </summary>
    public class ClubDetailDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }

        public string? Requirements { get; set; }
        public bool AllowAutoJoin { get; set; }
        public bool AllowMembersToPost { get; set; }

        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }

        public int CreatedByUserId { get; set; }

        // Mentor
        public int? MentorUserId { get; set; }
        public string? MentorName { get; set; }

        public int? PresidentUserId { get; set; }
        public string? PresidentName { get; set; }
        
        // User status flags
        public bool? IsMember { get; set; }
        public bool? IsPresident { get; set; }
        public bool? IsMentorInvite { get; set; }
        public bool IsRequestToJoin { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        
        // Member count (không load toàn bộ members)
        public int MemberCount { get; set; }
    }
}

