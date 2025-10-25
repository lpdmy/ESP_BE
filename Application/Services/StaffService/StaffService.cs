using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Infrastructure;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class StaffService : IStaffService
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _repo;
        public StaffService(IUserService userService, IMapper mapper, IUserRepository repo) {
            _userService = userService;
           _mapper = mapper;
            _repo = repo;
        }
        public async Task<PaginationResponseDto<UserResponseDto>> GetAllAsync(
    PaginationRequestDto paginationRequest)
        {
            var query = _repo.GetAllByStaffIncluding();

            if (!string.IsNullOrEmpty(paginationRequest.Search))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(paginationRequest.Search) ||
                    c.LastName.Contains(paginationRequest.Search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<UserResponseDto>>(data);
            var sql = query.ToQueryString();
            return new PaginationResponseDto<UserResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<UserResponseDto?> GetByIdAsync(int id)
        {
            var staff = await _repo.GetByStaffIdIncluding(id);
            if (staff == null)
            {
                throw new BadRequestException(ErrorMessages.Staff.StaffNotFound);
            }
            var mapped = _mapper.Map<UserResponseDto>(staff);
            return mapped;
        }
    }
}
