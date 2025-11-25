using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class ActivityJuryResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public int? ClubId { get; set; }
        public string Organizer { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public int MaxParticipants { get; set; }
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public int NumberOfParticipants { get; set; }
        public int numberOfSubmission { get; set; }
        public int numberOfPendingSubmission { get; set; }
        public int numberOfCompletedSubmission { get; set; }
    }
}
