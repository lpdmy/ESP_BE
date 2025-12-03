using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class UserViolationStatModel
    {
        public int AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public int ViolationCount { get; set; }
        public DateTime LatestViolation { get; set; }
    }
}
