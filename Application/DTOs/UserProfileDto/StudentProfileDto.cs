using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.UserProfileDto
{
    public class StudentProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? StudentNumber { get; set; }
        public short? EnrollmentYear { get; set; }
        public string? Bio { get; set; }
        public string? ExtraJson { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
