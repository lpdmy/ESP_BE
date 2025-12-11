using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.DTOs
{
    // Request DTO cho pagination sport rosters
    public class SportRosterPaginationRequestDto : PaginationRequestDto
    {
        [Required]
        public int ActivityId { get; set; }
        
        public int? SportId { get; set; } // Optional: filter by sport
    }

    // DTO cho một thành viên trong đội hình
    public class SportRosterMemberDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
    }

    // DTO cho một lớp trong một môn
    public class SportRosterClassDto
    {
        public int ClassGroupId { get; set; }
        public string ClassGroupName { get; set; } = string.Empty;
        public int? Grade { get; set; }
        public int MemberCount { get; set; }
        public List<SportRosterMemberDto> Members { get; set; } = new();
    }

    // DTO cho một môn với các lớp
    public class SportRosterDto
    {
        public int SportId { get; set; }
        public string SportName { get; set; } = string.Empty;
        public int? MaxMembers { get; set; }
        public List<SportRosterClassDto> Classes { get; set; } = new();
    }

    // Response DTO với pagination
    public class SportRosterPaginationResponseDto : PaginationResponseDto<SportRosterDto>
    {
    }
}

