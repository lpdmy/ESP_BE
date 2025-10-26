using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class RejectCreationDto
    {
        public int Id { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
