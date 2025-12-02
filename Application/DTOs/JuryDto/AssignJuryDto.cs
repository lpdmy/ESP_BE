using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class AssignJuryDto
    {
        public List<int> UserId { get; set; }
        public int SubmissionId { get; set; }
    }
}
