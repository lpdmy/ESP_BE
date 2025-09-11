
using AutoMapper;
using EduShpere.Application.DTOs;
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
        public async Task<IEnumerable<Activity>> GetAllAsync(int pageNumber, int pageSize, string? search =null)
        {
            var activities = await _repo.GetAllAsync();

            // Lọc theo Category
            var filteredActivities = activities
                .Where(c => c.Category == ActivityType.Event);

            // Nếu có search thì lọc thêm
            if (!string.IsNullOrEmpty(search))
            {
                filteredActivities = filteredActivities
                    .Where(c => c.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Áp dụng phân trang
            return filteredActivities
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

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
    }
}
