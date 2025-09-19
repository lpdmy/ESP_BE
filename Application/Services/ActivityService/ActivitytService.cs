
using AutoMapper;
using EduShpere.Application.DTOs;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Domain.Enum;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared;
using EduShpere.Shared.Constants;

namespace EduShpere.Application.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _repo;
        private readonly IMapper _mapper;
        public ActivityService(IActivityRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<(IEnumerable<Activity> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? search =null)
        {
            var activities = await _repo.GetAllAsync();
            var totalCount = activities.Count();
            if (!string.IsNullOrEmpty(search))
            {
                activities = activities
                    .Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

           var result = activities
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return (result,totalCount);
        }
        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }
        public async Task<ActivityResponseDto> AddAsync(CreateActivityDto dto)
        {
            if(dto.StartDate >= dto.EndDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDay);
            }
            var activity = new Activity
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                Category = dto.Category,
                SubType = dto.SubType,
                ThumbnailUrl = dto.ThumbnailUrl.FileName
            };

            await _repo.AddAsync(activity);
            var ActivityDtos = _mapper.Map<ActivityResponseDto>(activity);
            return ActivityDtos;
        }
        public async Task<ActivityResponseDto> UpdateAsync(UpdateActivityDto dto)
        {
            var existingActivity = await _repo.GetByIdAsync(dto.Id);
            if (existingActivity == null)
            {
                throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
            }
            if (dto.StartDate >= dto.EndDate)
            {
                throw new BadRequestException(ErrorMessages.Activity.StartDayAfterEndDay);
            }
            existingActivity.Title = dto.Title ?? existingActivity.Title;
            existingActivity.Description = dto.Description ?? existingActivity.Description;
            existingActivity.StartDate = dto.StartDate ?? existingActivity.StartDate;
            existingActivity.EndDate = dto.EndDate ?? existingActivity.EndDate;
            existingActivity.Location = dto.Location ?? existingActivity.Location;
            existingActivity.UpdatedAt = DateTime.Now;
            existingActivity.Category = dto.Category;
            existingActivity.SubType = dto.SubType ?? existingActivity.SubType;
            existingActivity.ThumbnailUrl = dto.ThumbnailUrl != null? dto.ThumbnailUrl.FileName : existingActivity.ThumbnailUrl;
            await _repo.UpdateAsync(existingActivity);
            var ActivityDtos = _mapper.Map<ActivityResponseDto>(existingActivity);
            return ActivityDtos;
        }
    }
}
