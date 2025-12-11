using EduShpere.Domain;
using EduShpere.Shared.Constants;
using System;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.UserDto
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = ErrorMessages.Validation.UsernameRequired)]
        [MaxLength(50)]
        public string Username { get; set; } = null!;

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
        public UserRole Role { get; set; } = UserRole.Student;

        // Thông tin profile (chỉ áp dụng cho Student)
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

        public int? ClassGroupId { get; set; }

        // Thông tin teacher profile (chỉ áp dụng cho Teacher)
        [StringLength(100, ErrorMessage = ErrorMessages.UserProfile.TeacherCodeTooLong)]
        public string? TeacherCode { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.UserProfile.DepartmentTooLong)]
        public string? Department { get; set; }

        [StringLength(100, ErrorMessage = ErrorMessages.UserProfile.PositionTooLong)]
        public string? Position { get; set; }
    }
}
