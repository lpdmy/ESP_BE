using EduShpere.Domain;
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
        public string? FullName => $"{FirstName} {LastName}".Trim();

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }

        public UserRole? Role { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
