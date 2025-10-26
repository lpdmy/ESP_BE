using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IClubCreationRequestService
    {
        Task<ClubCreationResponseDto> createClubRequest(CreateClubRequestDto dto, User user);
        Task<PaginationResponseDto<ClubCreationResponseDto>> GetAllAsync(
    PaginationRequestDto paginationRequest,
    string? search = null, int? status = null);
        Task<PaginationResponseDto<ClubCreationResponseDto>> GetAllAsyncByUser(User user,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<ClubCreationResponseDto> ApproveCreation(int id);
        Task<ClubCreationResponseDto> RejectCreation(RejectCreationDto rejectCreation);
    }
}
