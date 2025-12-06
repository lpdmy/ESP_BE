using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.ModerationDto
{
    public class CreateAlertDto
    {
        public int UserId { get; set; }
        public string? ContentText { get; set; }
    }
}
