using System.Text.Json;
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
    public class ClubCreationRequestService : IClubCreationRequestService
    {
        private readonly IClubCreationRepository _repo;
        private readonly IMapper _mapper;

        public ClubCreationRequestService(IClubCreationRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<ClubCreationResponseDto> createClubRequest(CreateClubRequestDto dto,User user)
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            var hasRecentRequest = await _repo.HasRecentSendCreationRequest(user.Id, oneWeekAgo);

            if (hasRecentRequest)
            {
                throw new BadRequestException(ErrorMessages.ClubCreationRequest.RecentRequestExists);
            }

            // 2. Tạo mới yêu cầu
            var clubRequest = new ClubCreationRequest
            {
                ClubName = dto.ClubName,
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                AllowMembersToPost = dto.AllowMembersToPost,
                Status = "Pending",
                AllowAutoJoin = dto.AllowAutoJoin,
                CategoryId = dto.CategoryId,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                AvatarUrl = dto.AvatarUrl,
                CoverUrl = dto.CoverUrl,
                Requirements = dto.Requirements,
                RequestedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id,
                IsDeleted = false,
            };
            await _repo.AddAsync(clubRequest);
          return  _mapper.Map<ClubCreationResponseDto>(clubRequest);
        }
        public async Task<PaginationResponseDto<ClubCreationResponseDto>> GetAllAsync(
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllWithIncludes();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Description.Contains(search) ||
                    c.RequestedByUser.LastName.Contains(search) ||
                    c.RequestedByUser.Email.Contains(search) || 
                    c.ClubName.Contains(search)); 
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClubCreationResponseDto>>(data);

            // Nếu muốn set thêm dữ liệu từ User
            foreach (var dto in mapped)
            {
                var request = data.FirstOrDefault(r => r.Id == dto.Id);
                if (request != null)
                {
                    dto.RequestedByName = request.RequestedByUser.LastName;
                    dto.RequestedByEmail = request.RequestedByUser.Email;
                }
            }

            return new PaginationResponseDto<ClubCreationResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

    }
}
