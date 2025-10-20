using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;

namespace EduShpere.Application.Services
{
    public interface IClubMemberService
    {
        Task<PaginationResponseDto<ClubMemberResponseDto>> GetAllByUserAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<ClubMemberResponseDto> OutClub(int clubId, int user);
    }
}
