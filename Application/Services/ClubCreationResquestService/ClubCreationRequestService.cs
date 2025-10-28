using System.Text.Json;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EduShpere.Application.Services
{
    public class ClubCreationRequestService : IClubCreationRequestService
    {
        private readonly IClubCreationRepository _repo;
        private readonly IMapper _mapper;
        private readonly IClubRepository _clubRepo;
        private readonly IClubMemberRepository _clubMemberRepo;
        public ClubCreationRequestService(IClubCreationRepository repo, IMapper mapper, IClubRepository clubRepo, IClubMemberRepository clubMemberRepo)
        {
            _repo = repo;
            _mapper = mapper;
            _clubRepo = clubRepo;
            _clubMemberRepo = clubMemberRepo;
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
    string? search = null,int ? status = null)
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

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == (status.Value == 1 ? "Pending" : status.Value == 2 ? "Approved" : "Rejected"));
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
        public async Task<PaginationResponseDto<ClubCreationResponseDto>> GetAllAsyncByUser( User user,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllWithIncludesByUser(user);
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();
            var mapped = _mapper.Map<IEnumerable<ClubCreationResponseDto>>(data);
            return new PaginationResponseDto<ClubCreationResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<ClubCreationResponseDto> ApproveCreation(int id)
        {
            var request = await _repo.GetByIdAsync(id);
            if (request == null)
            {
                throw new BadRequestException(ErrorMessages.ClubCreationRequest.RequestNotFound);
            }
            if (request.Status == "Approved")
            {
                throw new BadRequestException(ErrorMessages.ClubCreationRequest.AlreadyApproved);
            }
            var club = new Club
            {
                Name = request.ClubName,
                Description = request.Description,
                ShortDescription = request.ShortDescription,
                CategoryId = request.CategoryId,
                AvatarUrl = request.AvatarUrl,
                CoverUrl = request.CoverUrl,
                Requirements = request.Requirements,
                AllowAutoJoin = request.AllowAutoJoin,
                AllowMembersToPost = request.AllowMembersToPost,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = request.RequestedByUserId,
                PresidentUserId = request.RequestedByUserId,
            };
            await _clubRepo.AddAsync(club);

            var ClubMember = new ClubMember
            {
                ClubId = club.Id,
                UserId = request.RequestedByUserId,
                Role = "President",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.RequestedByUserId,
                IsDeleted = false,
            };
            await _clubMemberRepo.AddAsync(ClubMember);
            request.Status = "Approved";
            await _repo.UpdateAsync(request);
            var dto = _mapper.Map<ClubCreationResponseDto>(request);

            dto.RequestedByName = $"{request.RequestedByUser?.LastName} {request.RequestedByUser?.FirstName}";
            dto.RequestedByEmail = request.RequestedByUser?.Email;

            return dto;
        }
        public async Task<ClubCreationResponseDto> RejectCreation(RejectCreationDto rejectCreation)
        {
            var request = await _repo.GetByIdAsync(rejectCreation.Id);
            if (request == null)
            {
                throw new BadRequestException(ErrorMessages.ClubCreationRequest.RequestNotFound);
            }
            if (rejectCreation.Reason.IsNullOrEmpty())
            {
                throw new BadRequestException(ErrorMessages.ClubCreationRequest.NotNullReason);
            }
            request.Status = "Rejected";
            request.RejectReason = rejectCreation.Reason;
            await _repo.UpdateAsync(request);
            var dto = _mapper.Map<ClubCreationResponseDto>(request);
            dto.RequestedByName = $"{request.RequestedByUser?.LastName} {request.RequestedByUser?.FirstName}";
            dto.RequestedByEmail = request.RequestedByUser?.Email;
            return dto;
        }

    }
}
