using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using EduShpere.Application.DTOs;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;
using Microsoft.IdentityModel.Tokens;
using static EduShpere.Shared.Constants.ApiEndpoints;

namespace EduShpere.Application.Services
{
    public class JuryService : IJuryService
    {
        private readonly IJuryActivityRepository _juryActivityRepo;
        private readonly IMapper _mapper;
        private readonly IActivityRepository _activityRepo;
        public JuryService(IJuryActivityRepository juryActivityRepo, IMapper mapper,IActivityRepository activityRepository)
        {
            _juryActivityRepo = juryActivityRepo;
            _mapper = mapper;
            _activityRepo = activityRepository;
        }

        public async Task<List<JuryActivityResponseDto>> GetAllJuryByActivityIdAsync(int activityId, string? search)
        {
            var juryActivities = _juryActivityRepo
                .GetAllJuryActivityByActivityIdIncluding(activityId);
            if (juryActivities == null) { 
             throw new BadRequestException(ErrorMessages.Jury.JuryNotFound);
            }
            if (!search.IsNullOrEmpty())
            {
                juryActivities = juryActivities.Where(ja =>
                    ja.User.FirstName.Contains(search) ||
                    ja.User.LastName.Contains(search)
                );
            }
            var result = _mapper.Map<List<JuryActivityResponseDto>>(juryActivities);
            return result;
        }
        public async Task<List<JuryActivityResponseDto>> CreateJury(CreateJuryDto dto)
        {
            var activity = await _activityRepo.GetByIdWithIncludesAsync(dto.ActivityId);
            if(activity == null)
            {
                throw new BadRequestException(ErrorMessages.Activity.ActivityNotFound);
            }
            var result = new List<JuryActivityResponseDto>();
            foreach (var userId in dto.JuryId)
            {
                var isExisting = await _juryActivityRepo.IsExisting(userId, dto.ActivityId);
                if (!isExisting)
                {
                    var juryActivity = new JuryActivity
                    {
                        UserId = userId,
                        ActivityId = dto.ActivityId
                    };
                    await _juryActivityRepo.AddAsync(juryActivity);
                    result.Add(new JuryActivityResponseDto
                    {
                        UserId = juryActivity.UserId,
                        ActivityId = juryActivity.ActivityId,
                    });
                }
            }
            return result;
        }
        public async Task DeletedJury(int id)
        {
            var juryActivity = await _juryActivityRepo.GetByIdAsync(id);
            if (juryActivity == null)
            {
                throw new BadRequestException(ErrorMessages.Jury.JuryNotFound);
            }
            await _juryActivityRepo.DeleteAsync(juryActivity.Id);
        }
    }
}
