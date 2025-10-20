using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class ClubMemberService : IClubMemberService
    {
        private readonly IClubMemberRepository _repo;
        private readonly IMapper _mapper;
        private readonly IClubRepository _clubRepo;
        private readonly IClubJoinRequestRepository _clubJoinRequestRepo;
        public ClubMemberService(IClubMemberRepository repo, IMapper mapper, IClubRepository clubRepo, IClubJoinRequestRepository clubJoinRequestRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _clubRepo = clubRepo;
            _clubJoinRequestRepo = clubJoinRequestRepo;
        }
        public async Task<ClubMemberResponseDto> OutClub(int clubId, int userid)
        {
            var clubmember = await _repo.GetClubMemberByUserIdAndClubId(userid, clubId);
            if (clubmember == null)
            {
                throw new BadRequestException(ErrorMessages.ClubMember.NotMember);
            }
            await _repo.DeleteAsync(clubmember.Id);
            return _mapper.Map<ClubMemberResponseDto>(clubmember);
        }
        public async Task<PaginationResponseDto<ClubMemberResponseDto>> GetAllByUserAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetClubMemberByUser(user);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Club.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClubMemberResponseDto>>(data);
            var sql = query.ToQueryString();
            return new PaginationResponseDto<ClubMemberResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

    }
}
