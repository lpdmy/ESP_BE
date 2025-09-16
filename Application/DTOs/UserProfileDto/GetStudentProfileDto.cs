using System;

namespace EduShpere.Application.DTOs.UserProfileDto
{
    public class GetStudentProfileDto
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
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ClassGroupName { get; set; }
        public int? ClassGroupId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
