using System.ComponentModel.DataAnnotations;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.DTOs.AuthDto
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = ErrorMessages.Validation.EmailRequired)]
        [EmailAddress(ErrorMessage = ErrorMessages.Validation.EmailInvalidFormat)]
        public string Email { get; set; } = string.Empty;
    }
}
