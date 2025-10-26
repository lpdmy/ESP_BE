using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Infrastructure.Security;
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
        private readonly IUserRightRepository _userRightRepository;
        public StaffService(IUserService userService, IMapper mapper, IUserRepository repo, IUserRightRepository userRightRepository) {
            _userService = userService;
           _mapper = mapper;
            _repo = repo;
            _userRightRepository = userRightRepository;
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
        public async Task<UserResponseDto> UpdateStaff(UpdateStaffDto dto)
        {
            var staff = await _repo.GetByStaffIdIncluding(dto.Id);
            if (staff == null)
                throw new BadRequestException(ErrorMessages.Staff.StaffNotFound);
            staff.FirstName = dto.FirstName;
            staff.LastName = dto.LastName;
            staff.Email = dto.Email;
            staff.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password != staff.Password)
            {
                staff.Password = PasswordHelper.HashPassword(staff, dto.Password);
            }
            var currentRights = staff.UserRights
                .Where(ur => !ur.IsDeleted)
                .Select(ur => ur.RightId)
                .ToList();

            bool permissionsChanged = !dto.Permission.OrderBy(x => x).SequenceEqual(currentRights.OrderBy(x => x));
            var hasRight = await _userRightRepository.HasRight(staff.Id);
            if (permissionsChanged)
            {
                if (hasRight) { 
                await _userRightRepository.DeleteByUserIdAsync(staff.Id);

                }
                var newRights = dto.Permission.Select(rid => new UserRight
                {
                    UserId = staff.Id,
                    RightId = rid,
                    IsDeleted = false
                }).ToList();

                await _userRightRepository.AddRangeAsync(newRights);
            }
            staff.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(staff);
            staff = await _repo.GetByStaffIdIncluding(staff.Id);
            var mapped = _mapper.Map<UserResponseDto>(staff);
            return mapped;
        }

        public async Task DeleteSoft(int id)
        {
            var staff = await _repo.GetByStaffIdIncluding(id);
            if (staff == null)
            {
                throw new BadRequestException(ErrorMessages.Staff.StaffNotFound);
            }
            staff.IsDeleted = true;
            await _repo.UpdateAsync(staff);
        }
        public async Task RecoveryStaff(int id)
        {
            var staff = await _repo.GetByStaffIdIncluding(id);
            if (staff == null)
            {
                throw new BadRequestException(ErrorMessages.Staff.StaffNotFound);
            }
            staff.IsDeleted = false;
            await _repo.UpdateAsync(staff);
        }
    }
}
