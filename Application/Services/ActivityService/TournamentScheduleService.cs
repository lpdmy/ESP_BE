using System;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Application;
using EduShpere.Infrastructure;
using EduShpere.Infrastructure.Repositories;
using EduShpere.Shared.Constants;
using ESP.AIService.Services;
using ESP.AIService.Models;
using ESP.AIService.Entities;
using Microsoft.EntityFrameworkCore;
using EduShpere.Shared;

namespace EduShpere.Application.Services;

/// <summary>
/// Service wrapper để gọi AI scheduling service
/// </summary>
public class TournamentScheduleService : ITournamentScheduleService
{
    private readonly ESP.AIService.Services.TournamentScheduleService _aiService;
    private readonly IClassGroupRepository _classGroupRepository;

    public TournamentScheduleService(IClassGroupRepository classGroupRepository)
    {
        _aiService = new ESP.AIService.Services.TournamentScheduleService();
        _classGroupRepository = classGroupRepository;
    }

    public async Task<GenerateTournamentScheduleResponseDto> GenerateScheduleAsync(
        GenerateTournamentScheduleRequestDto request,
        EduShpereDbContext dbContext)
    {
        // Validation: StartDate >= Today
        var today = DateTime.UtcNow.Date;
        if (request.StartDate.Date < today)
        {
            throw new BadRequestException("Ngày bắt đầu phải lớn hơn hoặc bằng hôm nay.");
        }

        // Validation: StartDate <= EndDate
        if (request.StartDate > request.EndDate)
        {
            throw new BadRequestException("Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.");
        }

        // Filter ClassGroupIds: Chỉ lấy lớp của niên khóa hiện tại
        var currentAcademicYear = await _classGroupRepository.GetCurrentAcademicYearAsync();
        if (currentAcademicYear == null)
        {
            throw new BadRequestException("Không tìm thấy niên khóa hiện tại. Vui lòng liên hệ quản trị viên.");
        }

        // Lấy danh sách lớp thuộc niên khóa hiện tại
        var validClassGroups = await dbContext.ClassGroups
            .Where(cg => request.ClassGroupIds.Contains(cg.Id) && 
                        cg.AcademicYearId == currentAcademicYear.Id && 
                        !cg.IsDeleted)
            .Select(cg => cg.Id)
            .ToListAsync();

        if (validClassGroups.Count < 2)
        {
            throw new BadRequestException("Cần ít nhất 2 lớp thuộc niên khóa hiện tại để tạo lịch thi đấu.");
        }

        if (validClassGroups.Count < request.ClassGroupIds.Count)
        {
            var invalidCount = request.ClassGroupIds.Count - validClassGroups.Count;
            throw new BadRequestException(
                $"{invalidCount} lớp không thuộc niên khóa hiện tại ({currentAcademicYear.Name}). " +
                "Vui lòng chỉ chọn các lớp của niên khóa hiện tại.");
        }

        // Convert DTO sang AI service request (chỉ dùng validClassGroups)
        var aiRequest = new TournamentScheduleRequest
        {
            ActivityId = request.ActivityId,
            SportId = request.SportId,
            ClassGroupIds = validClassGroups, // Chỉ dùng lớp niên khóa hiện tại
            Grade = request.Grade,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MatchDuration = request.MatchDuration,
            PreferredStartTime = request.PreferredStartTime,
            PreferredEndTime = request.PreferredEndTime,
            AvailableLocations = request.AvailableLocations,
            MaxMatchesPerDay = request.MaxMatchesPerDay,
            MinGapBetweenMatches = request.MinGapBetweenMatches,
            TournamentFormat = request.TournamentFormat,
            UserNotes = request.UserNotes
        };

        // Gọi AI service
        // QUAN TRỌNG: Không dùng Task.Run vì DbContext không thread-safe
        // DbContext sẽ bị dispose nếu dùng trong Task.Run trên thread pool riêng
        var aiResponse = _aiService.GenerateSchedule(aiRequest, dbContext);

        // Convert AI response sang DTO
        return new GenerateTournamentScheduleResponseDto
        {
            Success = aiResponse.Success,
            IsOptimal = aiResponse.IsOptimal,
            GeneratedMatches = aiResponse.GeneratedMatches.Select(m => new AIActivityMatchDto
            {
                ActivityId = m.ActivityId,
                SportId = m.SportId,
                ClassGroup1Id = m.ClassGroup1Id,
                ClassGroup2Id = m.ClassGroup2Id,
                Grade = m.Grade,
                MatchDate = m.MatchDate,
                StartTime = m.StartTime?.ToString(@"hh\:mm"),
                EndTime = m.EndTime?.ToString(@"hh\:mm"),
                Location = m.Location,
                Status = (int)m.Status,
                Round = m.Round,
                RoundName = m.RoundName,
                MatchNumber = m.MatchNumber,
                NextMatchId = m.NextMatchId,
                IsBye = m.IsBye,
                Notes = m.Notes
            }).ToList(),
            Explanation = aiResponse.Explanation,
            ObjectiveValue = aiResponse.ObjectiveValue,
            TotalMatches = aiResponse.TotalMatches,
            TotalRounds = aiResponse.TotalRounds
        };
    }

    public async Task TrainModelAsync(EduShpereDbContext dbContext)
    {
        // QUAN TRỌNG: Không dùng Task.Run vì DbContext không thread-safe
        // Nếu cần chạy async, có thể dùng Task.CompletedTask hoặc chạy trực tiếp
        await Task.CompletedTask;
        _aiService.TrainModel(dbContext);
    }

    public async Task<ApplyTournamentScheduleResponseDto> ApplyScheduleAsync(
        int activityId,
        ApplyTournamentScheduleRequestDto request,
        EduShpereDbContext dbContext)
    {
        if (request.Matches == null || !request.Matches.Any())
        {
            throw new BadRequestException("Danh sách trận đấu không được để trống.");
        }

        await ActivityMatchSchemaHelper.EnsureIsPublishedColumnExistsAsync(dbContext);

        var activity = await dbContext.Activities
            .Include(a => a.ActivityMatches)
            .FirstOrDefaultAsync(a => a.Id == activityId && !a.IsDeleted);

        if (activity == null)
        {
            throw new NotFoundException(ErrorMessages.Activity.ActivityNotFound);
        }

        var normalizedMatches = new List<NormalizedMatch>();
        foreach (var match in request.Matches)
        {
            if (!match.MatchDate.HasValue)
            {
                throw new BadRequestException($"Match #{match.MatchNumber}: MatchDate không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(match.StartTime) || string.IsNullOrWhiteSpace(match.EndTime))
            {
                throw new BadRequestException($"Match #{match.MatchNumber}: StartTime và EndTime không được để trống.");
            }

            if (!TimeSpan.TryParse(match.StartTime, out var startTime) ||
                !TimeSpan.TryParse(match.EndTime, out var endTime))
            {
                throw new BadRequestException($"Match #{match.MatchNumber}: Thời gian không hợp lệ (định dạng HH:mm).");
            }

            if (startTime >= endTime)
            {
                throw new BadRequestException($"Match #{match.MatchNumber}: StartTime phải nhỏ hơn EndTime.");
            }

            normalizedMatches.Add(new NormalizedMatch
            {
                Dto = match,
                MatchDate = match.MatchDate.Value.Date,
                StartTime = startTime,
                EndTime = endTime,
                Location = match.Location?.Trim()
            });
        }

        // Validate duplicate match numbers
        var duplicateMatchNumber = normalizedMatches
            .GroupBy(m => m.Dto.MatchNumber)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicateMatchNumber != null)
        {
            throw new BadRequestException($"MatchNumber {duplicateMatchNumber.Key} bị trùng.");
        }

        // Validate overlaps
        for (int i = 0; i < normalizedMatches.Count; i++)
        {
            for (int j = i + 1; j < normalizedMatches.Count; j++)
            {
                var matchA = normalizedMatches[i];
                var matchB = normalizedMatches[j];

                if (matchA.MatchDate != matchB.MatchDate)
                    continue;

                var sameLocation = string.Equals(matchA.Location, matchB.Location, StringComparison.OrdinalIgnoreCase);
                if (!sameLocation && (!string.IsNullOrEmpty(matchA.Location) && !string.IsNullOrEmpty(matchB.Location)))
                {
                    continue;
                }

                if (matchA.StartTime < matchB.EndTime && matchA.EndTime > matchB.StartTime)
                {
                    throw new BadRequestException(
                        $"Trùng lịch giữa match #{matchA.Dto.MatchNumber} và match #{matchB.Dto.MatchNumber} " +
                        $"tại sân {(matchA.Location ?? "N/A")} ngày {matchA.MatchDate:dd/MM}.");
                }
            }
        }

        // Remove old matches (ghi đè)
        var existingMatches = await dbContext.ActivityMatches
            .Where(m => m.ActivityId == activityId)
            .ToListAsync();

        if (existingMatches.Any())
        {
            dbContext.ActivityMatches.RemoveRange(existingMatches);
            await dbContext.SaveChangesAsync();
        }

        var newMatches = new List<EduShpere.Domain.Models.ActivityMatch>();
        foreach (var normalizedMatch in normalizedMatches)
        {
            var dto = normalizedMatch.Dto;
            var entity = new EduShpere.Domain.Models.ActivityMatch
            {
                ActivityId = activityId,
                SportId = dto.SportId,
                ClassGroup1Id = dto.ClassGroup1Id,
                ClassGroup2Id = dto.ClassGroup2Id,
                Grade = dto.Grade,
                MatchDate = normalizedMatch.MatchDate,
                StartTime = normalizedMatch.StartTime,
                EndTime = normalizedMatch.EndTime,
                Location = normalizedMatch.Location,
                Status = dto.Status,
                Score1 = dto.Score1,
                Score2 = dto.Score2,
                WinnerClassGroupId = dto.WinnerClassGroupId,
                Round = dto.Round,
                RoundName = string.IsNullOrWhiteSpace(dto.RoundName) ? null : dto.RoundName.Trim(),
                MatchNumber = dto.MatchNumber,
                NextMatchId = null, // will be updated later
                IsBye = dto.IsBye,
                Notes = dto.Notes,
                IsPublished = request.IsPublished
            };

            newMatches.Add(entity);
        }

        await dbContext.ActivityMatches.AddRangeAsync(newMatches);
        await dbContext.SaveChangesAsync();

        // Map NextMatchNumber -> actual DB Id
        var numberToEntity = newMatches.ToDictionary(m => m.MatchNumber, m => m);
        foreach (var normalizedMatch in normalizedMatches)
        {
            if (normalizedMatch.Dto.NextMatchNumber.HasValue &&
                numberToEntity.TryGetValue(normalizedMatch.Dto.MatchNumber, out var currentEntity) &&
                numberToEntity.TryGetValue(normalizedMatch.Dto.NextMatchNumber.Value, out var nextEntity))
            {
                currentEntity.NextMatchId = nextEntity.Id;
            }
        }

        await dbContext.SaveChangesAsync();

        return new ApplyTournamentScheduleResponseDto
        {
            Success = true,
            IsPublished = request.IsPublished,
            TotalMatchesApplied = newMatches.Count,
            MatchIds = newMatches.Select(m => m.Id).ToList()
        };
    }

    private sealed class NormalizedMatch
    {
        public ApplyTournamentMatchDto Dto { get; set; } = default!;
        public DateTime MatchDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
    }
}

