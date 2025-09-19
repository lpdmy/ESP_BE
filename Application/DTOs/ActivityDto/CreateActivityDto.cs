using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Enum;
using Microsoft.AspNetCore.Http;

namespace EduShpere.Application.DTOs
{
    public class CreateActivityDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public string ThumbnailUrl { get; set; } = null!;
        [Required]
        public string Organizer { get; set; }
        [Required]
        public DateTime RegisterDate { get; set; }
        [Required]
        public DateTime EndRegisterDate { get; set; }
        [Required]
        public int MaxParticipants { get; set; }
    }
}
