using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShpere.Application.DTOs.ActivityDto
{
    /// <summary>
    /// DTO cho request trao giải thưởng dựa trên rank
    /// </summary>
    public class AwardRankRewardsRequestDto
    {
        [Required(ErrorMessage = "Participant ranks là bắt buộc")]
        public Dictionary<string, string> ParticipantRanks { get; set; } = new Dictionary<string, string>();
    }
}

