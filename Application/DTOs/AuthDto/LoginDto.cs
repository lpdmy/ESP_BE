
using System.ComponentModel.DataAnnotations;
using EduShpere.Shared.Constants;

namespace EduShpere.Application
{
    public class LoginDto
    {
        [Required(ErrorMessage = ErrorMessages.Validation.EmailRequired)]
        [EmailAddress(ErrorMessage = ErrorMessages.Validation.EmailInvalidFormat)]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = ErrorMessages.Validation.PasswordRequired)]
        public string Password { get; set; } = string.Empty;
    }
}
