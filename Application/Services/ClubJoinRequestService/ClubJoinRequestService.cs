using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.CommonDto;
using EduShpere.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;
using EduShpere.Shared.Constants;
using EduShpere.Infrastructure;
using EduShpere.Domain;
using EduShpere.Shared;
using EduShpere.Application.Services.NotificationService;
using EduShpere.Application.Services.StarPointService;
using EduShpere.Domain.Enum;

namespace EduShpere.Application.Services
{
    public class ClubJoinRequestService : IClubJoinRequestService
    {
        private readonly IClubJoinRequestRepository _repo;
        private readonly IMapper _mapper;
        private readonly IClubRepository _clubRepo;
        private readonly IClubMemberRepository _clubMemberRepo;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserActionRewardService _userActionRewardService;
        public ClubJoinRequestService(IClubJoinRequestRepository repo, IMapper mapper, IClubRepository clubRepo, IClubMemberRepository clubMemberRepo, IUserRepository userRepositor, INotificationService notificationService, IUserActionRewardService userActionRewardService)
        {
            _repo = repo;
            _mapper = mapper;
            _clubRepo = clubRepo;
            _clubMemberRepo = clubMemberRepo;
            _userRepository = userRepositor;
            _notificationService = notificationService;
            _userActionRewardService = userActionRewardService;
        }
        public async Task<PaginationResponseDto<ClubJoinRequestDto>> GetAllClubJoinRequestAsync(
     PaginationRequestDto paginationRequest,
     string? search = null)
        {
            var query = _repo.GetAllWithIncludes();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Club.Name.Contains(search) ||
                    c.User.FirstName.Contains(search) ||
                    c.User.LastName.Contains(search) ||
                    c.User.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClubJoinRequestDto>>(data);
            var sql = query.ToQueryString();
            Console.WriteLine(sql);
            return new PaginationResponseDto<ClubJoinRequestDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<PaginationResponseDto<ClubJoinRequestDto>> GetAllClubJoinRequestByCludId( int clubId,
     PaginationRequestDto paginationRequest,
     string? search = null)
        {
            var result = _repo.GetAllWithIncludes();
            var query = result.Where(c => c.ClubId == clubId && c.Status.Contains("Pending") && c.IsMentor != true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.User.Username.Contains(search) ||
                    c.User.FirstName.Contains(search) ||
                    c.User.LastName.Contains(search) ||
                    c.User.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClubJoinRequestDto>>(data);
            var sql = query.ToQueryString();
            Console.WriteLine(sql);
            return new PaginationResponseDto<ClubJoinRequestDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<ClubJoinRequestDto> CreateJoinRequest(CreateClubJoinRequestDto dto, User user)
        {
            var club = await _clubRepo.GetByIdAsync(dto.ClubId);
            if (club == null)
            {
                throw new Exception(ErrorMessages.Club.ClubNotFound);
            }
            var clubJoinRequest = new ClubJoinRequest
            {
                ClubId = dto.ClubId,
                UserId = user.Id,
                Status = "Pending",
                ReasonToJoin = dto.ReasonToJoin,
                Experience = dto.Experience
            };
            await _repo.AddAsync(clubJoinRequest);
            var mapped = _mapper.Map<ClubJoinRequestDto>(clubJoinRequest);
            return mapped;
        }
        public async Task<ClubJoinRequestDto> ApproveJoinRequest(int id)
        {
            var request = await _repo.GetByIdWithIncludesAsync(id);
            if (request == null)
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.RequestNotFound);
            }
            if (request.Status == "Approved")
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.AlreadyApproved);
            }
            request.Status = "Approved";
            var clubMember = new ClubMember
            {
                ClubId = request.ClubId,
                UserId = request.UserId,
                Role = "Member",
            };

            await _repo.UpdateAsync(request);
            await _clubMemberRepo.AddAsync(clubMember);

            // Cộng điểm cho hành động tham gia CLB
            await _userActionRewardService.AwardForActionAsync(request.UserId, RewardActionType.JoinClub);
            var mapped = _mapper.Map<ClubJoinRequestDto>(request);
            return mapped;
        }
        public async Task<bool> RejectJoinRequest(int id)
        {
            var request = await _repo.GetByIdAsync(id);
            if (request == null)
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.RequestNotFound);
            }
            request.Status = "Rejected";
            await _repo.UpdateAsync(request);
            return true;
        }
        public async Task<ClubJoinRequestDto> InviteMentor(InviteMentorDto dto,User sender)
        {
            var user = await _userRepository.GetByIdAsync(dto.MentorId);
            if (user == null || user.Role != UserRole.Teacher )
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.NotTeacher);
            }
            var request = new ClubJoinRequest
            {
                ClubId = dto.ClubId,
                UserId = dto.MentorId,
                Status = "Pending",
                IsMentor = true,
            };
            var isMentor = await _clubMemberRepo.IsMentorAnyClub(request.UserId);
            if (isMentor)
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.AlreadyMentor);
            }
            var isAlreadyInvite = await _repo.IsAlreadyInvite(request.UserId, request.ClubId);
            if (isAlreadyInvite) {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.AlreadyInvite);
            }
            await _notificationService.AddAsync(new Notification
            {
                Link = $"/club/{dto.ClubId}",
                Title = $"{user.FirstName} Đã gửi cho bạn lời mời vào Câu lạc bộ của họ",
                CreatedAt = DateTime.Now,
                Avatar = string.IsNullOrEmpty(sender.AvatarUrl) ? null : sender.AvatarUrl,
                Read = false,
                Type = "system",
                UserId = dto.MentorId
            });
            await _repo.AddAsync(request);
            var mapped = _mapper.Map<ClubJoinRequestDto>(request);
            return mapped;
        }
        public async Task<ClubJoinRequestDto> MentorApprove(int clubId,User user)
        {
            var request = await _repo.GetByUserIdAndClubId(clubId, user);
            if (request == null)
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.RequestNotFound);
            }
            request.Status = "Approved";
            await _repo.UpdateAsync(request);
            var club = await _clubRepo.GetByIdWithIncludesAsync(request.ClubId);
            if (club == null)
            {
                throw new BadRequestException(ErrorMessages.Club.ClubNotFound);
            }
            club.MentorUserId = request.UserId;
            await _clubRepo.UpdateAsync(club);
            await _notificationService.AddAsync(new Notification
            {
                Title = $"{club.President.FirstName} Đã gửi cho bạn lời mời vào Câu lạc bộ của họ",
                CreatedAt = DateTime.Now,
                Avatar = string.IsNullOrEmpty(club.President.AvatarUrl) ? null : club.President.AvatarUrl,
                Read = false,
                Type = "system",
                UserId = request.UserId,
            });
            var clubMember = new ClubMember
            {
                ClubId = request.ClubId,
                UserId = request.UserId,
                Role = "Mentor",
            };
            
            await _clubMemberRepo.AddAsync(clubMember);

            // Mentor tham gia CLB cũng được tính là tham gia CLB
            await _userActionRewardService.AwardForActionAsync(request.UserId, RewardActionType.JoinClub);
            var mapped = _mapper.Map<ClubJoinRequestDto>(request);
            return mapped;
        }
        public async Task<ClubJoinRequestDto> CancelJoinRequest(int clubId ,User user) {
            var request = await _repo.GetByUserIdAndClubId(clubId, user);
            if (request == null)
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.RequestNotFound);
            }
            await _repo.DeleteAsync(request.Id);
            var mapped = _mapper.Map<ClubJoinRequestDto>(request);
            return mapped;
        }

        public async Task<PaginationResponseDto<ClubJoinRequestDto>> GetAllClubJoinRequestByUserByClubId( int userid,int clubid,
     PaginationRequestDto paginationRequest,
     string? search = null)
        {
            var query = _repo.GetAllWithIncludesByUser(userid);
            query = query.Where(c => c.ClubId == clubid);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Club.Name.Contains(search));
                   
            }
            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClubJoinRequestDto>>(data);
            var sql = query.ToQueryString();
            Console.WriteLine(sql);
            return new PaginationResponseDto<ClubJoinRequestDto>
            {
                Data = mapped,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }
        public async Task<ClubJoinRequestDto> GetMentorInviationByClubId(int clubId)
        {
            var request = await _repo.GetMentorInviationByClubId(clubId);
            if (request == null)
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.RequestNotFound);
            }
            var mapped = _mapper.Map<ClubJoinRequestDto>(request);
            return mapped;
        }
        public async Task CancelInvitationMentor(int id)
        {
            var invitation = await _repo.GetByIdAsync(id);
            if (invitation == null)
            {
                throw new BadRequestException(ErrorMessages.ClubJoinRequest.RequestNotFound);
            }
            await _repo.CancelMentorInvitaion(id);
        }
    }
}
