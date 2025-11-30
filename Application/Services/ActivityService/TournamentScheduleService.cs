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
        var aiResponse = await Task.Run(() => _aiService.GenerateSchedule(aiRequest, dbContext));

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
        await Task.Run(() => _aiService.TrainModel(dbContext));
    }
}

