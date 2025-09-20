using EduShpere.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.UserProfileDto
{
    public class CreateUpdateTeacherProfileDto
    {
        [Required(ErrorMessage = ErrorMessages.Validation.UserIdRequired)]
        public int UserId { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.UserProfile.TeacherCodeTooLong)]
        public string? TeacherCode { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.UserProfile.DepartmentTooLong)]
        public string? Department { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.UserProfile.PositionTooLong)]
        public string? Position { get; set; }

        [StringLength(1000, ErrorMessage = ErrorMessages.UserProfile.BioTooLong)]
        public string? Bio { get; set; }

        [StringLength(500, ErrorMessage = ErrorMessages.UserProfile.AvatarUrlTooLong)]
        public string? AvatarUrl { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [StringLength(20, ErrorMessage = ErrorMessages.UserProfile.PhoneNumberTooLong)]
        public string? PhoneNumber { get; set; }

        public string? ExtraJson { get; set; }
    }
}