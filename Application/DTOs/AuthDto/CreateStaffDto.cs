using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.DTOs
{
    public class CreateStaffDto
    {

        [Required(ErrorMessage = ErrorMessages.Validation.EmailRequired)]
        [EmailAddress(ErrorMessage = ErrorMessages.Validation.EmailInvalidFormat)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessages.Validation.FirstNameRequired)]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = null!;
        public List<int>? Permission { get; set; }
    }
}
