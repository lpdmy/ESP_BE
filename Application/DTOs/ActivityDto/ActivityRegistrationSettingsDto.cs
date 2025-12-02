using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto;

public class ActivityRegistrationSettingsDto
{
    public GroupRegistrationSettingsDto? GroupRegistration { get; set; }
}

public class GroupRegistrationSettingsDto
{
    [Range(1, 100, ErrorMessage = "Số thành viên tối thiểu phải từ 1 - 100")]
    public int MinMembers { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Số thành viên tối đa phải từ 1 - 100")]
    public int? MaxMembers { get; set; } // null means unlimited

    public bool RequireLeader { get; set; } = true;
}

