using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs
{
    public class CreateJuryDto
    {
        public List<int> JuryId { get; set; }
        public int ActivityId { get; set; }
    }
}
