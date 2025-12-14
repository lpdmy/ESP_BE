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
        /// Cộng điểm cho user với transaction (cập nhật UserPoint.Balance + tạo PointHistory)
        /// Đảm bảo tính toàn vẹn dữ liệu giữa việc cập nhật số dư và lưu lịch sử
        /// </summary>
        public async Task<bool> AddPointsWithTransactionAsync(int userId, int points, string description, PointActionType actionType = PointActionType.Earn)
        {
            if (points <= 0)
            {
                throw new ArgumentException("Points must be greater than 0", nameof(points));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lấy hoặc tạo UserPoint record
                var userPoint = await _context.UserPoint
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (userPoint == null)
                {
                    // Tạo mới UserPoint nếu chưa có
                    userPoint = new UserPoint
                    {
                        UserId = userId,
                        Balance = 0,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false,
                        RowVersion = new byte[8] // Initialize RowVersion
                    };
                    await _context.UserPoint.AddAsync(userPoint);
                }

                // 2. Cập nhật Balance
                if (actionType == PointActionType.Earn)
                {
                    userPoint.Balance += points;
                }
                else if (actionType == PointActionType.Redeem)
                {
                    if (userPoint.Balance < points)
                    {
                        throw new InvalidOperationException($"Insufficient balance. Current: {userPoint.Balance}, Required: {points}");
                    }
                    userPoint.Balance -= points;
                }
                else // Adjustment
                {
                    userPoint.Balance = points; // Set directly for adjustment
                }

                userPoint.UpdatedAt = DateTime.UtcNow;

                // 3. Tạo PointHistory record
                var pointHistory = new PointHistory
                {
                    UserId = userId,
                    ActionType = actionType,
                    Points = points,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.PointHistory.AddAsync(pointHistory);

                // 4. Save changes
                await _context.SaveChangesAsync();

                // 5. Commit transaction
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                // Rollback transaction on error
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
