using EduShpere.Application.DTOs.StarPointDto;
using EduShpere.Domain.Models;
using EduShpere.Domain.Enum;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories.StarPoint;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
            var points = await _context.Set<PointHistory>()
            .Where(p => p.UserId == userId)
            .SumAsync(p =>
                p.ActionType == Domain.Enum.PointActionType.Earn
                    ? p.Points
                    : -p.Points
            );

            return new UserPointsDto { UserId = userId, Points = points };
        }

        /// <summary>
        /// Cộng điểm cho user với transaction (chỉ tạo PointHistory record)
        /// PointHistory là nguồn dữ liệu chính để lưu trữ point, balance được tính từ tổng PointHistory
        /// Sử dụng execution strategy để tương thích với SqlServerRetryingExecutionStrategy
        /// </summary>
        public async Task<bool> AddPointsWithTransactionAsync(int userId, int points, string description, PointActionType actionType = PointActionType.Earn)
        {
            if (points <= 0)
            {
                throw new ArgumentException("Points must be greater than 0", nameof(points));
            }

            // Kiểm tra balance nếu là Redeem
            if (actionType == PointActionType.Redeem)
                {
                var currentBalance = await GetCurrentUserPoints(userId);
                if (currentBalance.Points < points)
                    {
                    throw new InvalidOperationException($"Insufficient balance. Current: {currentBalance.Points}, Required: {points}");
                    }
            }

            // Sử dụng execution strategy để tương thích với retry policy
            var executionStrategy = _context.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Chỉ tạo PointHistory record - đây là nguồn dữ liệu chính
                var pointHistory = new PointHistory
                {
                    UserId = userId,
                    ActionType = actionType,
                    Points = points,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.PointHistory.AddAsync(pointHistory);

                    // Save changes
                await _context.SaveChangesAsync();

                    // Commit transaction
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
                throw;
            }
            });
        }
    }
}
