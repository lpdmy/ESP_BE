using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.SubmissionDto
{
    public class ClassGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public string Grade { get; set; }
        public string Class => $"{Grade}{Name}";
    }
}
