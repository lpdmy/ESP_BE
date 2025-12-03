using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;

namespace EduShpere.Application.Services
{
    public interface IActivityParticipantService
    {
        Task<ActivityParticipantResponseDto> AddActivityParticipant(AddParticipantDto dto);
        Task<GroupRegistrationResultDto> RegisterGroupAsync(GroupRegistrationDto dto);
        Task<SportRegistrationResultDto> RegisterSportAsync(SportRegistrationDto dto);
        Task<ActivityParticipantResponseDto> RemoveActivityParticipant(int participationId);
        Task<bool> CancelRegistrationAsync(int activityId, int userId);
        Task<int> CountNumberParticipantInActivity(int activityId);
        Task<SportRosterPaginationResponseDto> GetSportRostersAsync(SportRosterPaginationRequestDto request);
    }
}
