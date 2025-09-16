using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Application.DTOs.AuthDto
{
    public class ChangePasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}
