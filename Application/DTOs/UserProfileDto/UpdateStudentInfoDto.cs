using EduShpere.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.UserProfileDto
{
    public class UpdateStudentInfoDto
    {
        public int UserId { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.UserProfile.StudentNumberTooLong)]
        public string? StudentNumber { get; set; }

        [Range(2000, 2030, ErrorMessage = ErrorMessages.UserProfile.InvalidEnrollmentYear)]
        public short? EnrollmentYear { get; set; }

        [StringLength(1000, ErrorMessage = ErrorMessages.UserProfile.BioTooLong)]
        public string? Bio { get; set; }

        public string? ExtraJson { get; set; }

        [Url(ErrorMessage = ErrorMessages.UserProfile.InvalidAvatarUrl)]
        [StringLength(500, ErrorMessage = ErrorMessages.UserProfile.AvatarUrlTooLong)]
        public string? AvatarUrl { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Phone(ErrorMessage = ErrorMessages.UserProfile.InvalidPhoneNumber)]
        [StringLength(20, ErrorMessage = ErrorMessages.UserProfile.PhoneNumberTooLong)]
        public string? PhoneNumber { get; set; }
    }
}
