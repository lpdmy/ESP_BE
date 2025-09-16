using System.ComponentModel.DataAnnotations;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.DTOs.AuthDto
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = ErrorMessages.Validation.OldPasswordRequired)]
        public string OldPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = ErrorMessages.Validation.NewPasswordRequired)]
        [MinLength(6, ErrorMessage = ErrorMessages.Password.PasswordTooShort)]
        [MaxLength(20, ErrorMessage = ErrorMessages.Password.PasswordTooLong)]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = ErrorMessages.Validation.ConfirmPasswordRequired)]
        [Compare(nameof(NewPassword), ErrorMessage = ErrorMessages.Password.PasswordMismatch)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
