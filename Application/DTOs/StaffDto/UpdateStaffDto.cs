using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Shared.Constants;

namespace EduShpere.Application
{
    public class UpdateStaffDto
    {
        public int Id { get; set; }
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
