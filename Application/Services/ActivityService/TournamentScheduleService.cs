using System;
using System.Linq;
using System.Threading.Tasks;
using EduShpere.Application.DTOs.ActivityDto;
using EduShpere.Infrastructure;
using ESP.AIService.Services;
using ESP.AIService.Models;
using ESP.AIService.Entities;

namespace EduShpere.Application.Services;

/// <summary>
/// Service wrapper để gọi AI scheduling service
/// </summary>
public class TournamentScheduleService : ITournamentScheduleService
{
    private readonly ESP.AIService.Services.TournamentScheduleService _aiService;

    public TournamentScheduleService()
    {
        _aiService = new ESP.AIService.Services.TournamentScheduleService();
    }

    public async Task<GenerateTournamentScheduleResponseDto> GenerateScheduleAsync(
        GenerateTournamentScheduleRequestDto request,
        EduShpereDbContext dbContext)
    {
        // Convert DTO sang AI service request
        var aiRequest = new TournamentScheduleRequest
        {
            ActivityId = request.ActivityId,
            SportId = request.SportId,
            ClassGroupIds = request.ClassGroupIds,
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

