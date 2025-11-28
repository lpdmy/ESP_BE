using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs;

public class SportRegistrationDto
{
    [Required]
    public int ActivityId { get; set; }

    [Required]
    public int SportId { get; set; }

    [Required]
    public int ClassGroupId { get; set; }

    [Required]
    public List<int> MemberIds { get; set; } = new();

    public int? RequestedByUserId { get; set; }
}

public class SportRegistrationResultDto
{
    public int SportId { get; set; }
    public int ClassGroupId { get; set; }
    public IEnumerable<ActivityParticipantResponseDto> Participants { get; set; } = new List<ActivityParticipantResponseDto>();
}

