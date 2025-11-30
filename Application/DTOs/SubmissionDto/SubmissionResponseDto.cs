using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Models;
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
        public int NumberJurys { get; set; }
        public ClassGroupDto Class { get; set; }
        public List<int> Users { get; set; }
        public string? FileUrl { get; set; }
        public string? GradeSettings { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Score { get; set; }
        public string ActivityName { get; set; }
        public string Status { get; set; }
        public List<JuryAssignmentDto> JuryAssignments { get; set; }

    }
}
