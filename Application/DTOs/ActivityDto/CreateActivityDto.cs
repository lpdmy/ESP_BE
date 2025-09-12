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
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public ActivityType Category { get; set; }
        public string SubType { get; set; } = null!;
        public IFormFile ThumbnailUrl { get; set; } = null!;
    }
}
