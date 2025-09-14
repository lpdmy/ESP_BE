using EduShpere.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.UserProfileDto
{
    public class CreateUpdateStudentProfileDto
    {
        public int UserId { get; set; }
        public string? StudentNumber { get; set; }
        public string? Bio { get; set; }
        public string? ExtraJson { get; set; }
        public string? AvatarUrl { get; set; }
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
