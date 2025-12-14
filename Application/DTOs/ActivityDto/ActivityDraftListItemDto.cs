using System;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class ActivityDraftListItemDto
    {
        public int Id { get; set; }

        public string? DraftName { get; set; }

        public string? Title { get; set; }

        public ActivityType Category { get; set; }

        public string SubType { get; set; } = null!;

        public string? ThumbnailUrl { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}

