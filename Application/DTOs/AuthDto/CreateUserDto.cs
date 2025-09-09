using EduShpere.Domain;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.UserDto
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
        public UserRole Role { get; set; } = UserRole.Student;
    }
}
