using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class Right : BaseEntity
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
        public virtual ICollection<UserRight> UserRights { get; set; } = new List<UserRight>();
    }
}
