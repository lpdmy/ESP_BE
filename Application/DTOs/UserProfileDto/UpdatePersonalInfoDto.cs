using EduShpere.Shared.Constants;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.UserProfileDto
{
    public class UpdatePersonalInfoDto
    {
        [Phone(ErrorMessage = ErrorMessages.UserProfile.InvalidPhoneNumber)]
        [StringLength(20, ErrorMessage = ErrorMessages.UserProfile.PhoneNumberTooLong)]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Url(ErrorMessage = ErrorMessages.UserProfile.InvalidAvatarUrl)]
        [StringLength(500, ErrorMessage = ErrorMessages.UserProfile.AvatarUrlTooLong)]
        public string? AvatarUrl { get; set; }

        [StringLength(1000, ErrorMessage = ErrorMessages.UserProfile.BioTooLong)]
        public string? Bio { get; set; }

        public string? ExtraJson { get; set; }
    }
}
