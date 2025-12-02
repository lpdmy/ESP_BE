using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs;

public class GroupRegistrationDto
{
    [Required]
    public int ActivityId { get; set; }

    [Required]
    public int LeaderId { get; set; }

    [Required]
    public List<int> MemberIds { get; set; } = new();

    public int? ClassGroupId { get; set; }

    public string? GroupName { get; set; }

    public int? RequestedByUserId { get; set; }
}

public class GroupRegistrationResultDto
{
    public Guid GroupCode { get; set; }
    public IEnumerable<ActivityParticipantResponseDto> Participants { get; set; } = new List<ActivityParticipantResponseDto>();
}

