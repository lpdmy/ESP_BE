using EduShpere.Application.DTOs.StarPointDto;
using EduShpere.Domain.Models;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories.StarPoint;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services.StarPointService
{
    public class PointHistoryService : IPointHistoryService
    {
        private readonly IPointHistoryRepository _repo;
        private readonly ICurrentUserService _currentUserService;
        private readonly EduShpereDbContext _context;

        public PointHistoryService(IPointHistoryRepository repo, ICurrentUserService currentUserService, EduShpereDbContext context)
        {
            _repo = repo;
            _currentUserService = currentUserService;
            _context = context;
        }

        public async Task<List<PointHistoryDto>> GetUserHistoryAsync()
        {
            var userId = _currentUserService.GetCurrentUserId() ?? 0;
            var data = await _repo.GetByUserIdAsync(userId);
            return data.Select(p => new PointHistoryDto
            {
                Id = p.Id,
                UserId = userId,
                UserName = p.User?.LastName!,
                ActionType = p.ActionType.ToString(),
                Points = p.Points,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<PointHistoryDto> CreateHistoryAsync(CreatePointHistoryDto dto)
        {
            var entity = new PointHistory
            {
                UserId = _currentUserService.GetCurrentUserId() ?? 0,
                ActionType = dto.ActionType,
                Points = dto.Points,
                Description = dto.Description,
                CreatedAt = DateTime.Now
            };
            var added = await _repo.AddAsync(entity);
            return new PointHistoryDto
            {
                Id = added.Id,
                UserId = added.UserId,
                ActionType = added.ActionType.ToString(),
                Points = added.Points,
                Description = added.Description,
                CreatedAt = added.CreatedAt
            };
        }

        public async Task<UserPointsDto> GetCurrentUserPoints(int userId)
        {
            var points = await _context.PointHistory
            .Where(p => p.UserId == userId)
            .SumAsync(p =>
                p.ActionType == Domain.Enum.PointActionType.Earn
                    ? p.Points
                    : -p.Points
            );

            return new UserPointsDto { UserId = userId, Points = points };
        }
    }
}
