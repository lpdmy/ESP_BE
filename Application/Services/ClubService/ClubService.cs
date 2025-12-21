using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Application.DTOs;
using EduShpere.Infrastructure.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using EduShpere.Domain.Models;
using EduShpere.Domain;
using EduShpere.Infrastructure;
using EduShpere.Application.DTOs.SearchDto;
using Microsoft.IdentityModel.Tokens;
using EduShpere.Application.Services.NotificationService;

namespace EduShpere.Application.Services
{
    public class ClubService : IClubService
    {
        private readonly IClubRepository _repo;
        private readonly IMapper _mapper;
        private readonly IClubJoinRequestRepository _clubJoinRequestRepo;
        private readonly IClubCategoryRepository _clubCategoryRepo;
        private readonly IClubMemberRepository _clubMemberRepo;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        public ClubService(IClubRepository repo, IMapper mapper, IClubJoinRequestRepository clubJoinRequestRepo, INotificationService notificationService, IClubCategoryRepository clubCategoryRepo, IClubMemberRepository clubMemberRepo, IUserRepository userRepository) {
            _repo = repo;
            _mapper = mapper;
            _clubJoinRequestRepo = clubJoinRequestRepo;
            _clubCategoryRepo = clubCategoryRepo;
            _clubMemberRepo = clubMemberRepo;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }
        public async Task<PaginationResponseDto<ClubResponseDto>> GetAllAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllWithIncludes();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Description.Contains(search) ||
                    c.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClubResponseDto>>(data);
            foreach (var dto in mapped)
            {
                var club = data.First(c => c.Id == dto.Id);
                dto.IsMember = club.ClubMembers.Any(m => m.UserId == user.Id);
                dto.IsPresident = club.PresidentUserId == user.Id;
            }
            var sql = query.ToQueryString();
            return new PaginationResponseDto<ClubResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<ClubResponseDto>> GetAllAdminAsync(User user,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {
            var query = _repo.GetAllWithAdminIncludes();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.Description.Contains(search) ||
                    c.Name.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClubResponseDto>>(data);
            foreach (var dto in mapped)
            {
                var club = data.First(c => c.Id == dto.Id);
                dto.IsMember = club.ClubMembers.Any(m => m.UserId == user.Id);
                dto.IsPresident = club.PresidentUserId == user.Id;
            }
            var sql = query.ToQueryString();
            return new PaginationResponseDto<ClubResponseDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<ClubResponseDto> GetClubByID(int id,User user)
        {
            var club = await _repo.GetByIdWithIncludesAsync(id);
            if (club == null)
            {
                throw new BadRequestException(ErrorMessages.Club.ClubNotFound);
            }

            var mapped = _mapper.Map<ClubResponseDto>(club);
            mapped.IsPresident = club.PresidentUserId == user.Id;
            mapped.IsMember = club.ClubMembers.Any(m => m.UserId == user.Id);
            mapped.IsRequestToJoin = club.ClubJoinRequests.Any(r => r.UserId == user.Id && r.Status == "Pending");
            mapped.IsMentorInvite = club.ClubJoinRequests.Any(r => r.UserId == user.Id && r.IsMentor== true && r.Status == "Pending");
            return mapped;
        }
        public async Task<ClubResponseDto> UpdateClub(UpdateClubDto dto)
        {
            var club = await _repo.GetByIdWithIncludesAsync(dto.Id);
            if (club == null)
            {
                throw new BadRequestException(ErrorMessages.Club.ClubNotFound);
            }
            if (dto.CoverUrl.IsNullOrEmpty()) { 
            dto.CoverUrl = club.CoverUrl;
            }
            if (dto.AvatarUrl.IsNullOrEmpty())
            {
                dto.AvatarUrl = club.AvatarUrl;
            }
            club.Name = dto.Name;
            club.Description = dto.Description;
            club.ShortDescription = dto.ShortDescription;
            club.AllowMembersToPost = dto.AllowMembersToPost;
            club.AllowAutoJoin = dto.AllowAutoJoin;
            club.CategoryId = dto.CategoryId;
            club.ContactEmail = dto.ContactEmail;
            club.ContactPhone = dto.ContactPhone;
            club.AvatarUrl = dto.AvatarUrl;
            club.CoverUrl = dto.CoverUrl;
            club.Requirements = dto.Requirements;
            club.MentorUserId = dto.MentorUserId;
            club.PresidentUserId = dto.PresidentUserId;
            club.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(club);
            var mapped = _mapper.Map<ClubResponseDto>(club);
            return mapped;
        }
        public async Task<bool> DeleteClub(int id)
        {
            var club = await _repo.GetByIdWithIncludesAsync(id);
            if (club == null)
            {
                throw new BadRequestException(ErrorMessages.Club.ClubNotFound);
            }
            await _notificationService.AddAsync(new Notification
            {
                Title = $"Câu lạc bộ {club.Name} của bạn đã bị tạm ngừng ",
                CreatedAt = DateTime.Now,
                Read = false,
                Type = "system",
                UserId = club.ClubMembers.Where(p=>p.Role.Equals("President")).Select(p=>p.UserId).FirstOrDefault(),
            });
            await _repo.DeleteSoft(club.Id);
            return true;
        }
        public async Task<ClubResponseDto> CreateClub(CreateClubDto dto)
        {
            var club = new Club
            {
                Name = dto.Name,
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                AllowMembersToPost = dto.AllowMembersToPost,
                AllowAutoJoin = dto.AllowAutoJoin,
                CategoryId = dto.CategoryId,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                AvatarUrl = dto.AvatarUrl,
                CoverUrl = dto.CoverUrl,
                Requirements = dto.Requirements,
                MentorUserId = dto.MentorUserId,
                PresidentUserId = dto.PresidentUserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
            };
            await _notificationService.AddAsync(new Notification
            {
                Title = "Một đơn đăng ký thành lập câu lạc bộ mới cần được duyệt",
                CreatedAt = DateTime.Now,
                Read = false,
                Type = "system",
                UserId = 21,
            });
            await _repo.AddAsync(club);
            var mapped = _mapper.Map<ClubResponseDto>(club);
            return mapped;
        }
        public async Task<IEnumerable<ClubCategory>> GetAllClubCategory() { 
        return await _clubCategoryRepo.GetAllAsync();
        }
        public async Task<PaginationResponseDto<UserSearchResultDto>> GetAllAsync(int role,
    PaginationRequestDto paginationRequest,
    string? search = null)
        {

            var roleEnum = role switch
            {
                1 => UserRole.Admin,
                2 => UserRole.Teacher,
                4 => UserRole.Student,
                _ => throw new ArgumentException("Role không hợp lệ")
            };
            var users = _userRepository.GetAllIncluding();
            var query = users.Where(p => p.Role == roleEnum);
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search) || 
                    c.Username.Contains(search) ||
                    c.Email.Contains(search)) ;
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<UserSearchResultDto>>(data);
            
            var sql = query.ToQueryString();
            return new PaginationResponseDto<UserSearchResultDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
    }
}
