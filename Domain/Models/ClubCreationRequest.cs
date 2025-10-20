using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduShpere.Domain.Models
{
    public class ClubCreationRequest : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RequestedByUserId { get; set; }
        public string ClubName { get; set; }
        [Required]
        [StringLength(255)]

        public string? Description { get; set; }
        [StringLength(255)]
        public string? ShortDescription { get; set; }
        public int CategoryId { get; set; }

        [StringLength(1000)]
        public string? AvatarUrl { get; set; }

        [StringLength(1000)]
        public string? CoverUrl { get; set; }
        public string? Requirements { get; set; }
        public bool AllowAutoJoin { get; set; } = false;
        public bool AllowMembersToPost { get; set; } = false;
        [StringLength(255)]
        public string? ContactEmail { get; set; }

        [StringLength(20)]
        public string? ContactPhone { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected

        [StringLength(255)]
        public string? RejectReason { get; set; }

        [ForeignKey("RequestedByUserId")]
        public virtual User RequestedByUser { get; set; } = null!;
        [ForeignKey("CategoryId")]
        public virtual ClubCategory Category { get; set; } = null!;
    }

}
