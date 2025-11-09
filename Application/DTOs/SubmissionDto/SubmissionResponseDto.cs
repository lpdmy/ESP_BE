using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EduShpere.Application.DTOs.SubmissionDto
{
    public class SubmissionResponseDto
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string UserFullName => $"{LastName} {FirstName}";
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ClassGroupDto Class { get; set; }
    }
}
