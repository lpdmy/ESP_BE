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
        var dto = new GenerateTournamentScheduleResponseDto
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
            TotalRounds = aiResponse.TotalRounds,
            SlotWarnings = (aiResponse?.SlotWarnings ?? new List<SlotWarningReason>())
                .Select(w => new SlotWarningReasonDto
                {
                    WarningType = w.WarningType,
                    Title = w.Title,
                    Description = w.Description,
                    CurrentValue = w.CurrentValue,
                    RecommendedValue = w.RecommendedValue,
                    Solution = w.Solution,
                    Severity = w.Severity,
                    Field = w.Field ?? string.Empty
                }).ToList(),
            SlotConflicts = (aiResponse?.SlotConflicts ?? new List<ESP.AIService.Models.SlotConflictDetail>())
                .Select(c => new SlotConflictDetailDto
                {
                    Slot = new SlotInfoDto
                    {
                        MatchDate = c.Slot.MatchDate,
                        StartTime = c.Slot.StartTime.ToString(@"hh\:mm"),
                        EndTime = c.Slot.EndTime.ToString(@"hh\:mm"),
                        Location = c.Slot.Location
                    },
                    ConflictType = c.ConflictType,
                    Reason = c.Reason,
                    MatchConflict = c.MatchConflict != null ? new MatchConflictInfoDto
                    {
                        MatchId = c.MatchConflict.MatchId,
                        MatchNumber = c.MatchConflict.MatchNumber,
                        ClassGroup1Id = c.MatchConflict.ClassGroup1Id,
                        ClassGroup2Id = c.MatchConflict.ClassGroup2Id,
                        MatchDate = c.MatchConflict.MatchDate,
                        StartTime = c.MatchConflict.StartTime.ToString(@"hh\:mm"),
                        EndTime = c.MatchConflict.EndTime.ToString(@"hh\:mm"),
                        Location = c.MatchConflict.Location,
                        Description = c.MatchConflict.Description
                    } : null,
                    ActivityConflict = c.ActivityConflict != null ? new ActivityConflictInfoDto
                    {
                        ActivityId = c.ActivityConflict.ActivityId,
                        ActivityTitle = c.ActivityConflict.ActivityTitle,
                        StartDate = c.ActivityConflict.StartDate,
                        EndDate = c.ActivityConflict.EndDate,
                        Description = c.ActivityConflict.Description
                    } : null,
                    ParticipantConflicts = c.ParticipantConflicts.Select(p => new ParticipantConflictInfoDto
                    {
                        UserId = p.UserId,
                        UserName = p.UserName,
                        ClassGroupId = p.ClassGroupId,
                        ClassGroupName = p.ClassGroupName
                    }).ToList()
                }).ToList()
        };

        // Chuẩn hóa RoundName cho vòng cuối: luôn là "Chung kết" nếu không còn NextMatch
        if (dto.GeneratedMatches.Any())
        {
            var maxRound = dto.GeneratedMatches.Max(m => m.Round);
            foreach (var match in dto.GeneratedMatches.Where(m => m.Round == maxRound && !m.NextMatchId.HasValue))
            {
                match.RoundName = "Chung kết";
            }
        }

        return dto;
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

        // ========================
        // Conflict checking (cross-activity & class schedules)
        // ========================
        var conflicts = new List<ScheduleConflictDto>();

        // Lấy danh sách ClassGroupId duy nhất trong matches
        var matchClassGroupIds = normalizedMatches
            .SelectMany(m => new[] { m.Dto.ClassGroup1Id, m.Dto.ClassGroup2Id })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        // Guard: nếu không có class groups, bỏ qua conflict check nâng cao
        if (matchClassGroupIds.Any())
        {
            // Lấy participants của activity hiện tại (guard quan trọng: chỉ check cho người thực sự tham gia activity này)
            var activityParticipants = await dbContext.ActivityParticipants
                .Where(ap => ap.ActivityId == activityId && !ap.IsDeleted)
                .ToListAsync();

            var participantsByClassGroup = activityParticipants
                .Where(ap => ap.ClassGroupId.HasValue)
                .GroupBy(ap => ap.ClassGroupId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(ap => ap.UserId).ToList());

            // Chỉ lấy schedules cho các lớp có participants trong activity này
            var classGroupsWithParticipants = participantsByClassGroup.Keys.ToList();

            if (classGroupsWithParticipants.Any())
            {
                // Lịch học chính khóa cho các lớp tham gia
                var classSchedules = await dbContext.ClassGroupSchedules
                    .Where(s => classGroupsWithParticipants.Contains(s.ClassGroupId))
                    .ToListAsync();

                var schedulesByClassGroup = classSchedules
                    .GroupBy(s => s.ClassGroupId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Các hoạt động khác mà participants đã tham gia (cross-activity conflict)
                var participantUserIds = activityParticipants
                    .Select(ap => ap.UserId)
                    .Distinct()
                    .ToList();

                if (participantUserIds.Any())
                {
                    var otherActivities = await dbContext.Activities
                        .Include(a => a.ActivityParticipants)
                        .Where(a =>
                            a.Id != activityId &&
                            !a.IsDeleted &&
                            a.StartDate.HasValue &&
                            a.EndDate.HasValue &&
                            a.ActivityParticipants.Any(ap =>
                                !ap.IsDeleted &&
                                participantUserIds.Contains(ap.UserId)))
                        .ToListAsync();

                    // Duyệt từng match để kiểm tra conflict chi tiết
                    foreach (var nm in normalizedMatches)
                    {
                        var dto = nm.Dto;
                        var matchStart = nm.MatchDate.Add(nm.StartTime);
                        var matchEnd = nm.MatchDate.Add(nm.EndTime);

                        // Cross-activity conflicts (A)
                        foreach (var classGroupId in new[] { dto.ClassGroup1Id, dto.ClassGroup2Id }.Where(id => id.HasValue).Select(id => id!.Value))
                        {
                            if (!participantsByClassGroup.TryGetValue(classGroupId, out var userIdsForClass) ||
                                !userIdsForClass.Any())
                            {
                                // Lớp không có học sinh tham gia activity này → bỏ qua theo guard clause
                                continue;
                            }

                            foreach (var otherActivity in otherActivities)
                            {
                                var otherStart = otherActivity.StartDate!.Value;
                                var otherEnd = otherActivity.EndDate!.Value;

                                if (matchStart < otherEnd && matchEnd > otherStart)
                                {
                                    // Tìm các học sinh thực sự bị conflict
                            var conflictedUserIds = otherActivity.ActivityParticipants
                                        .Where(ap =>
                                            !ap.IsDeleted &&
                                            userIdsForClass.Contains(ap.UserId))
                                        .Select(ap => ap.UserId)
                                        .Distinct()
                                        .ToList();

                                    foreach (var userId in conflictedUserIds)
                                    {
                                        conflicts.Add(new ScheduleConflictDto
                                        {
                                            ConflictType = "ActivityConflict",
                                            ActivityId = otherActivity.Id,
                                            UserId = userId,
                                            ClassGroupId = classGroupId,
                                            MatchNumber = dto.MatchNumber,
                                            MatchDate = nm.MatchDate,
                                            StartTime = nm.StartTime.ToString(@"hh\:mm"),
                                            EndTime = nm.EndTime.ToString(@"hh\:mm"),
                                            Message = $"Người dùng {userId} (lớp {classGroupId}) đã tham gia hoạt động #{otherActivity.Id} trong cùng khoảng thời gian."
                                        });
                                    }
                                }
                            }
                        }

                        // Class schedule conflicts (B)
                        foreach (var classGroupId in new[] { dto.ClassGroup1Id, dto.ClassGroup2Id }.Where(id => id.HasValue).Select(id => id!.Value))
                        {
                            if (!schedulesByClassGroup.TryGetValue(classGroupId, out var schedulesForClass) ||
                                !schedulesForClass.Any())
                            {
                                continue;
                            }

                            var dayOfWeek = nm.MatchDate.DayOfWeek;
                            var dayOfWeekNormalized = dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;

                            foreach (var schedule in schedulesForClass.Where(s => s.DayOfWeek == dayOfWeekNormalized))
                            {
                                var scheduleStart = nm.MatchDate.Date.Add(schedule.StartTime);
                                var scheduleEnd = nm.MatchDate.Date.Add(schedule.EndTime);

                                if (matchStart < scheduleEnd && matchEnd > scheduleStart)
                                {
                                    conflicts.Add(new ScheduleConflictDto
                                    {
                                        ConflictType = "ClassScheduleConflict",
                                        ActivityId = activityId,
                                        ClassGroupId = classGroupId,
                                        MatchNumber = dto.MatchNumber,
                                        MatchDate = nm.MatchDate,
                                        StartTime = nm.StartTime.ToString(@"hh\:mm"),
                                        EndTime = nm.EndTime.ToString(@"hh\:mm"),
                                        Message = $"Trận #{dto.MatchNumber} trùng với lịch học của lớp {classGroupId} (tiết {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm})."
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        // Nếu có conflict chi tiết → trả về response với HTTP 409 (được controller map)
        if (conflicts.Any())
        {
            return new ApplyTournamentScheduleResponseDto
            {
                Success = false,
                IsPublished = false,
                TotalMatchesApplied = 0,
                MatchIds = new List<int>(),
                Conflicts = conflicts
            };
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

