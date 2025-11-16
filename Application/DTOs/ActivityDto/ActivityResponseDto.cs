
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;

namespace EduShpere.Application.DTOs
{
    public class ActivityResponseDto : BaseEntity
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public int? ClubId { get; set; }
        public string Organizer { get; set; }
        public bool IsDeleted { get; set; }
        public int MaxParticipants { get; set; }
        public byte[] RowVersion { get; set; } = null!;
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        public int numberOfParticipants { get; set; }
        public DateTime RegisterDate { get; set; }
        public DateTime EndRegisterDate { get; set; }
        public IEnumerable<ActivityParticipantDto> Participants { get; set; } = null!;
        public List<string> Rules { get; set; } = new();
        public int numberOfSubmission { get; set; }
        public int numberOfPendingSubmission { get; set; }
        public int numberOfCompletedSubmission { get; set; }
    }

}
