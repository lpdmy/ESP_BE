using System.Collections.Generic;

namespace EduShpere.Application.DTOs.ActivityDto;

public class RecentActivityInputsDto
{
    public List<string> Locations { get; set; } = new();
    public List<string> Organizers { get; set; } = new();
    public List<string> Contacts { get; set; } = new();
}

