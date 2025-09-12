using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.DTOs
{
    public class ActivityParticipantResponseDto
    {
        public int Id { get; set; }

        public int ActivityId { get; set; }

        public int UserId { get; set; }

        public ParticipantStatus? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; }

        public byte[] RowVersion { get; set; } = null!;
    }
}
