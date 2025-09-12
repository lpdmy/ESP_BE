

using EduShpere.Application.DTOs;

namespace EduShpere.Application.Services
{
    public interface IActivityParticipantService
    {
        Task<ActivityParticipantResponseDto> AddActivityParticipant(AddParticipantDto dto);
    }
}
