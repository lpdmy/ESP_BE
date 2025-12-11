using EduShpere.Domain;
using EduShpere.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.AuthDto
{
    public class UserDto
    {
        public int Id { get; set; }
        public long? SchoolId { get; set; }

        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{LastName} {FirstName}".Trim();

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }

        public UserRole? Role { get; set; }
        public UserStatus? Status { get; set; }

        // Student Profile fields
        public string? StudentNumber { get; set; }
        public int? EnrollmentYear { get; set; }
        public int? Grade { get; set; }
        public int? ClassGroupId { get; set; }
        public string? ClassName { get; set; }

        // Teacher Profile fields
        public string? TeacherCode { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}
