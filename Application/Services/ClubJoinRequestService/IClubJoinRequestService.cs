using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IClubJoinRequestService
    {
        Task<PaginationResponseDto<ClubJoinRequestDto>> GetAllClubJoinRequestAsync(
     PaginationRequestDto paginationRequest,
     string? search = null);
        Task<ClubJoinRequestDto> CreateJoinRequest(CreateClubJoinRequestDto dto, User user);
        Task<ClubJoinRequestDto> ApproveJoinRequest(int id);
        Task<bool> RejectJoinRequest(int id);
        Task<ClubJoinRequestDto> InviteMentor(InviteMentorDto dto);
        Task<ClubJoinRequestDto> MentorApprove(int id);
        Task<PaginationResponseDto<ClubJoinRequestDto>> GetAllClubJoinRequestByCludId(int clubId,
     PaginationRequestDto paginationRequest,
     string? search = null);
        Task<ClubJoinRequestDto> CancelJoinRequest(int clubId, User user);
    }
}
