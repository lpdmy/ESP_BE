using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application.DTOs.ActivityDto
{
    public class UpdateActivityDto
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
        public DateTime RegisterDate { get; set; }
        public DateTime EndRegisterDate { get; set; }
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
    }
}
