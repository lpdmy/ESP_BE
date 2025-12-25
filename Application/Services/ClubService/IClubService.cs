using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs;
using EduShpere.Domain;
using EduShpere.Domain.Models;
using EduShpere.Application.DTOs.SearchDto;

namespace EduShpere.Application.Services
{
    public interface IClubService
    {
        Task<PaginationResponseDto<ClubResponseDto>> GetAllAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<ClubResponseDto> GetClubByID(int id,User user);
         Task<ClubDetailDto> GetClubDetailAsync(int id, User user);
        Task<PaginationResponseDto<ClubMemberDto>> GetClubMembersAsync(int clubId, PaginationRequestDto paginationRequest, string? search = null);
        Task<ClubResponseDto> UpdateClub(UpdateClubDto dto);
        Task<ClubResponseDto> CreateClub(CreateClubDto dto);
        Task<bool> DeleteClub(int id);
        Task<IEnumerable<ClubCategory>> GetAllClubCategory();
        Task<PaginationResponseDto<UserSearchResultDto>> GetAllAsync(int role,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<PaginationResponseDto<ClubResponseDto>> GetAllAdminAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null);
        Task<bool> RestoreClub(int id);
    }
}
