using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.ActivityDto;

namespace EduShpere.Application.DTOs
{
    public class JuryActivityResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserFullName => $"{LastName} {FirstName}";
        public ActivityResponseDto? Activity { get; set; }
        public int Assigned { get; set; }
        public string? Avatar { get; set; }
    }
}
