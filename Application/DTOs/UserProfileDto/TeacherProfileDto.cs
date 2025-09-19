namespace EduShpere.Application.DTOs.UserProfileDto
{
    public class TeacherProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? TeacherCode { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Bio { get; set; }
        public string? ExtraJson { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}