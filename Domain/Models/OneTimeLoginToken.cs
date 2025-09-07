using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class OneTimeLoginToken
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        public string Token { get; set; } = null!;

        public DateTime Expiry { get; set; }

        public bool IsUsed { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }

}
