using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Infrastructure;

namespace EduShpere.Application.Services;

/// <summary>
/// Service để tạo lịch thi đấu bằng AI
/// </summary>
public interface ITournamentScheduleService
{
    /// <summary>
    /// Tạo lịch thi đấu cho activity
    /// </summary>
    Task<GenerateTournamentScheduleResponseDto> GenerateScheduleAsync(
        GenerateTournamentScheduleRequestDto request,
        EduShpereDbContext dbContext);

    /// <summary>
    /// Train ML model từ dữ liệu matches đã có
    /// </summary>
    Task TrainModelAsync(EduShpereDbContext dbContext);

    /// <summary>
    /// Apply lịch thi đấu sau khi người dùng chỉnh sửa
    /// </summary>
    Task<ApplyTournamentScheduleResponseDto> ApplyScheduleAsync(
        int activityId,
        ApplyTournamentScheduleRequestDto request,
        EduShpereDbContext dbContext);
}

