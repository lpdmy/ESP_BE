using EduShpere.Domain;
using EduShpere.Shared.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.AuthDto
{
    public class UpdateUserDto
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string? Username { get; set; } = null!;

        [EmailAddress(ErrorMessage = ErrorMessages.Validation.EmailInvalidFormat)]
        public string? Email { get; set; } = null!;

        [MaxLength(100)]
        public string? FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string? LastName { get; set; } = null!;


        [MaxLength(15)]
        public string? PhoneNumber { get; set; } = null!;
        public DateTime? Birthdate { get; set; } = null!;
        public string? Address { get; set; } = null!;
        public string? AvatarUrl { get; set; } = null!;

        public UserRole? Role { get; set; } = null!;
    }
}
