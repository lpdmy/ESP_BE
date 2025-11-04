using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain
{
    public enum UserRole : byte
    {
        Admin = 0,
        Staff = 1,
        Student = 4,
        Teacher = 2,
    }
}
