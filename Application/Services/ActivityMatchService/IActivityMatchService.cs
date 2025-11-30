using EduShpere.Application.DTOs.ActivityMatchDto;

namespace EduShpere.Application.Services
{
    public interface IActivityMatchService
    {
        Task<BracketResponseDto> GenerateSingleEliminationBracketAsync(GenerateBracketDto dto);
        Task<BracketResponseDto> GetBracketByActivityAsync(int activityId, int sportId, int? grade = null);
        Task<IEnumerable<MatchResponseDto>> GetMatchesByRoundAsync(int activityId, int sportId, int round, int? grade = null);
        Task<MatchResponseDto> GetMatchByIdAsync(int matchId);
        Task<MatchResponseDto> CreateMatchAsync(CreateMatchDto dto);
        Task<MatchResponseDto> UpdateMatchResultAsync(int matchId, UpdateMatchResultDto dto);
        Task<MatchResponseDto> UpdateMatchAsync(int matchId, UpdateMatchDto dto);
        Task<bool> DeleteBracketAsync(int activityId, int sportId, int? grade = null);
        Task<bool> DeleteMatchAsync(int matchId);
        Task<IEnumerable<EligibleClassGroupsByGradeDto>> GetEligibleClassGroupsAsync(int activityId, int sportId);
    }
}

