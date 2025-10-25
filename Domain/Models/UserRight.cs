using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class UserRight : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RightId { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
        public virtual User User { get; set; }
        public virtual Right Right { get; set; }
    }
}
