
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class ActivityParticipantService : IActivityParticipantService
    {
        private readonly IActivityParticipantRepository _repo;
        private readonly IMapper _mapper;
        public ActivityParticipantService(IActivityParticipantRepository repo, IMapper mapper) {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<ActivityParticipantResponseDto> AddActivityParticipant(AddParticipantDto dto) {
            if (dto.UserId == null || dto.ActivityId == null) {
                throw new BadRequestException("Dữ liệu đầu vào không được rỗng");
            }
            bool isJoin = await _repo.isAlreadyRegistered(dto.UserId, dto.ActivityId);
            if (isJoin == true) {
                throw new BadRequestException(ErrorMessages.ActivityParticipant.AlreadyJoined);
            }

            var participant = new ActivityParticipant
            {
                ActivityId = dto.ActivityId,
                UserId = dto.UserId,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                CreatedBy = dto.UserId,
                Status = ParticipantStatus.Joined,
                
            };
            await _repo.AddAsync(participant);
            var participantDto = _mapper.Map<ActivityParticipantResponseDto>(participant);
            return participantDto;
        }
        public async Task<ActivityParticipantResponseDto> RemoveActivityParticipant(int participationId)
        {
            var participant = await _repo.GetByIdAsync(participationId);
            if (participant == null)
            {
                throw new NotFoundException(ErrorMessages.ActivityParticipant.NotFound);
            }
            await _repo.SoftDeleteAsync(participationId);
            var participantDto = _mapper.Map<ActivityParticipantResponseDto>(participant);
            return participantDto;
        }

    }
}
