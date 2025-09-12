
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs
{
    public class ActivityResponseDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }


        public string? Location { get; set; }

        public int? ClubId { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }

        public byte[] RowVersion { get; set; } = null!;
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public IEnumerable<ActivityParticipantDto> Participants { get; set; } = null!;
    }
}
