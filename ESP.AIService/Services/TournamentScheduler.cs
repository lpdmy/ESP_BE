using System;
using System.Collections.Generic;
using System.Linq;
using ESP.AIService.Models;
using EduShpere.Infrastructure;
using EduShpere.Domain.Models;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace ESP.AIService.Services;

/// <summary>
/// Scheduler đơn giản, rõ ràng để tạo lịch thi đấu cho hội thao
/// - Check trùng timetable (lịch học) bằng EF Core
/// - Check trùng các trận đấu khác
/// - Check trùng sân thi đấu nếu có nhiều sân
/// - Tối ưu cho lớp sáng/chiều
/// </summary>
public class TournamentScheduler
{
    private readonly EduShpereDbContext _dbContext;

    public TournamentScheduler(EduShpereDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Tạo lịch thi đấu cho tournament
    /// </summary>
    public TournamentScheduleResponse GenerateSchedule(
        TournamentScheduleRequest request,
        List<ScheduleSlot> slots)
    {
        var response = new TournamentScheduleResponse
        {
            Success = false,
            IsOptimal = false,
            Explanation = string.Empty,
            GeneratedMatches = new List<ActivityMatch>()
        };

        // Validate input
        if (!slots.Any())
        {
            response.Explanation = "Không có slots available.";
            return response;
        }

        if (!request.ClassGroupIds.Any())
        {
            response.Explanation = "Không có lớp nào tham gia.";
            return response;
        }

        // 1. Load dữ liệu cần thiết từ DB bằng EF Core
        var existingMatches = LoadExistingMatches(request.ActivityId);
        var classSchedules = LoadClassSchedules(request.ClassGroupIds);
        
        // 2. Filter slots: loại bỏ slots conflict
        var availableSlots = FilterAvailableSlots(
            slots, 
            existingMatches, 
            classSchedules, 
            request);

        if (!availableSlots.Any())
        {
            response.Explanation = "Không có slots nào available sau khi filter conflicts. " +
                                "Vui lòng mở rộng khoảng thời gian hoặc kiểm tra lại conflicts.";
            response.SlotWarnings = GenerateWarnings(0, request, existingMatches.Count);
            return response;
        }

        // 3. Phân loại lớp sáng/chiều
        var classAnalysis = AnalyzeClasses(request.ClassGroupIds, classSchedules);

        // 4. Tính số matches cần tạo
        var expectedMatches = CalculateExpectedMatches(
            request.ClassGroupIds.Count, 
            request.TournamentFormat);

        // 5. Generate matches
        List<ActivityMatch> matches;
        if (request.TournamentFormat.ToLower() == "singleelimination")
        {
            matches = GenerateSingleEliminationMatches(
                request, 
                availableSlots, 
                classAnalysis, 
                classSchedules);
        }
        else if (request.TournamentFormat.ToLower() == "roundrobin")
        {
            matches = GenerateRoundRobinMatches(
                request, 
                availableSlots, 
                classAnalysis, 
                classSchedules);
        }
        else
        {
            matches = GenerateSingleEliminationMatches(
                request, 
                availableSlots, 
                classAnalysis, 
                classSchedules);
        }

        // 6. Sắp xếp matches theo thời gian và đánh lại số
        matches = SortAndRenumberMatches(matches);

        // 7. Build response
        response.Success = matches.Count > 0;
        response.IsOptimal = matches.Count >= expectedMatches;
        response.GeneratedMatches = matches;
        response.TotalMatches = matches.Count;
        response.TotalRounds = matches.Any() ? matches.Max(m => m.Round) : 0;

        if (matches.Count < expectedMatches)
        {
            response.Explanation = $"Đã tạo {matches.Count}/{expectedMatches} trận đấu. " +
                                 "Không đủ slots để tạo đủ tất cả trận đấu.";
        }
        else
        {
            response.Explanation = $"Đã tạo thành công {matches.Count} trận đấu trong {response.TotalRounds} vòng.";
        }

        response.SlotWarnings = GenerateWarnings(
            availableSlots.Count, 
            request, 
            existingMatches.Count);

        return response;
    }

    #region Data Loading (EF Core)

    /// <summary>
    /// Load các trận đấu đã có (trừ activity hiện tại) bằng EF Core
    /// </summary>
    private List<ActivityMatch> LoadExistingMatches(int currentActivityId)
    {
        return _dbContext.Set<ActivityMatch>()
            .Where(m => m.ActivityId != currentActivityId
                     && m.MatchDate != null
                     && m.StartTime != null
                     && m.EndTime != null
                     && m.Status != MatchStatus.Cancelled
                     && !m.IsDeleted)
            .ToList();
    }

    /// <summary>
    /// Load lịch học của các lớp bằng EF Core (cả ClassGroupSchedule và Timetable)
    /// </summary>
    private Dictionary<int, List<ClassScheduleInfo>> LoadClassSchedules(List<int> classGroupIds)
    {
        var result = new Dictionary<int, List<ClassScheduleInfo>>();

        // Load từ ClassGroupSchedule
        var classGroupSchedules = _dbContext.Set<ClassGroupSchedule>()
            .Where(s => classGroupIds.Contains(s.ClassGroupId) && !s.IsDeleted)
            .ToList();

        foreach (var schedule in classGroupSchedules)
        {
            if (!result.ContainsKey(schedule.ClassGroupId))
            {
                result[schedule.ClassGroupId] = new List<ClassScheduleInfo>();
            }

            result[schedule.ClassGroupId].Add(new ClassScheduleInfo
            {
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Subject = schedule.Subject
            });
        }

        // Load từ Timetable
        try
        {
            var timetables = _dbContext.Set<Timetable>()
                .Where(t => classGroupIds.Contains(t.ClassGroupId) && !t.IsDeleted)
                .ToList();

            foreach (var timetable in timetables)
            {
                if (!result.ContainsKey(timetable.ClassGroupId))
                {
                    result[timetable.ClassGroupId] = new List<ClassScheduleInfo>();
                }

                result[timetable.ClassGroupId].Add(new ClassScheduleInfo
                {
                    DayOfWeek = timetable.DayOfWeek,
                    StartTime = timetable.StartTime,
                    EndTime = timetable.EndTime,
                    Subject = timetable.SubjectName
                });
            }
        }
        catch
        {
            // Timetable table có thể chưa tồn tại, bỏ qua
        }

        return result;
    }

    #endregion

    #region Slot Filtering

    /// <summary>
    /// Filter slots: loại bỏ các slots conflict với:
    /// - Trận đấu đã có (cùng sân và overlap thời gian)
    /// - Lịch học của các lớp (sẽ check khi assign match cụ thể)
    /// - Location không trong danh sách available
    /// </summary>
    private List<ScheduleSlot> FilterAvailableSlots(
        List<ScheduleSlot> slots,
        List<ActivityMatch> existingMatches,
        Dictionary<int, List<ClassScheduleInfo>> classSchedules,
        TournamentScheduleRequest request)
    {
        var availableSlots = new List<ScheduleSlot>();
        var hasMultipleLocations = request.AvailableLocations != null && request.AvailableLocations.Count >= 2;

        foreach (var slot in slots)
        {
            bool isValid = true;

            // 1. Check location có trong danh sách available không
            if (request.AvailableLocations.Any())
            {
                if (!string.IsNullOrEmpty(slot.Location) && 
                    !request.AvailableLocations.Contains(slot.Location))
                {
                    continue; // Location không hợp lệ
                }
            }

            // 2. Check conflict với trận đấu đã có
            foreach (var existingMatch in existingMatches)
            {
                if (existingMatch.MatchDate == null || 
                    existingMatch.StartTime == null || 
                    existingMatch.EndTime == null)
                    continue;

                var existingDate = existingMatch.MatchDate.Value.Date;
                var existingStart = existingMatch.StartTime.Value;
                var existingEnd = existingMatch.EndTime.Value;

                // Cùng ngày và overlap thời gian
                if (slot.MatchDate.Date == existingDate &&
                    slot.StartTime < existingEnd && 
                    slot.EndTime > existingStart)
                {
                    // Nếu có nhiều sân, chỉ conflict nếu cùng sân
                    if (!hasMultipleLocations || 
                        (slot.Location == existingMatch.Location))
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            // 3. Check khung giờ cấm (11:30 - 13:00 nghỉ trưa)
            if (isValid)
            {
                var slotStartMinutes = slot.StartTime.Hours * 60 + slot.StartTime.Minutes;
                var slotEndMinutes = slot.EndTime.Hours * 60 + slot.EndTime.Minutes;
                var forbiddenStart = 11 * 60 + 30; // 11:30
                var forbiddenEnd = 13 * 60; // 13:00

                if (slotStartMinutes < forbiddenEnd && slotEndMinutes > forbiddenStart)
                {
                    isValid = false;
                }
            }

            // 4. Check PreferredStartTime và PreferredEndTime
            if (isValid)
            {
                if (request.PreferredStartTime.HasValue)
                {
                    if (slot.StartTime < request.PreferredStartTime.Value)
                    {
                        isValid = false;
                    }
                }

                if (request.PreferredEndTime.HasValue)
                {
                    if (slot.EndTime > request.PreferredEndTime.Value)
                    {
                        isValid = false;
                    }
                }
            }

            if (isValid)
            {
                availableSlots.Add(slot);
            }
        }

        return availableSlots.OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).ToList();
    }

    #endregion

    #region Class Analysis

    /// <summary>
    /// Phân loại lớp sáng/chiều dựa trên lịch học
    /// </summary>
    private ClassAnalysis AnalyzeClasses(
        List<int> classGroupIds,
        Dictionary<int, List<ClassScheduleInfo>> classSchedules)
    {
        var analysis = new ClassAnalysis
        {
            MorningClasses = new HashSet<int>(),
            AfternoonClasses = new HashSet<int>()
        };

        foreach (var classId in classGroupIds)
        {
            if (!classSchedules.TryGetValue(classId, out var schedules) || !schedules.Any())
            {
                // Không có lịch học, mặc định là buổi sáng
                analysis.MorningClasses.Add(classId);
                continue;
            }

            bool hasMorning = false;
            bool hasAfternoon = false;

            foreach (var schedule in schedules)
            {
                var startHour = schedule.StartTime.Hours;
                
                // Buổi sáng: 6h - 12h
                if (startHour >= 6 && startHour < 12)
                {
                    hasMorning = true;
                }
                
                // Buổi chiều: 12h - 18h
                if (startHour >= 12 && startHour < 18)
                {
                    hasAfternoon = true;
                }
            }

            // Phân loại: chủ yếu học buổi nào
            if (hasMorning && !hasAfternoon)
            {
                analysis.MorningClasses.Add(classId);
            }
            else if (hasAfternoon && !hasMorning)
            {
                analysis.AfternoonClasses.Add(classId);
            }
            else
            {
                // Có cả sáng và chiều, đếm số tiết
                var morningCount = schedules.Count(s => s.StartTime.Hours >= 6 && s.StartTime.Hours < 12);
                var afternoonCount = schedules.Count(s => s.StartTime.Hours >= 12 && s.StartTime.Hours < 18);
                
                if (morningCount > afternoonCount)
                {
                    analysis.MorningClasses.Add(classId);
                }
                else
                {
                    analysis.AfternoonClasses.Add(classId);
                }
            }
        }

        return analysis;
    }

    #endregion

    #region Conflict Checking

    /// <summary>
    /// Check xem slot có conflict với lịch học của 2 lớp không
    /// </summary>
    private bool HasScheduleConflict(
        ScheduleSlot slot,
        int? classGroup1Id,
        int? classGroup2Id,
        Dictionary<int, List<ClassScheduleInfo>> classSchedules,
        int minGapMinutes)
    {
        if (!classGroup1Id.HasValue && !classGroup2Id.HasValue)
            return false;

        var dayOfWeek = (int)slot.MatchDate.DayOfWeek;
        var dayOfWeekNormalized = dayOfWeek == 0 ? 7 : dayOfWeek; // Sunday = 7

        // Check lớp 1
        if (classGroup1Id.HasValue && 
            classSchedules.TryGetValue(classGroup1Id.Value, out var schedules1))
        {
            foreach (var schedule in schedules1)
            {
                if (schedule.DayOfWeek == dayOfWeekNormalized)
                {
                    // Check overlap hoặc quá gần (cần ít nhất minGapMinutes phút)
                    var gapBefore = (schedule.StartTime - slot.EndTime).TotalMinutes;
                    var gapAfter = (slot.StartTime - schedule.EndTime).TotalMinutes;

                    if (gapBefore < minGapMinutes && gapAfter < minGapMinutes)
                    {
                        return true; // Conflict
                    }
                }
            }
        }

        // Check lớp 2
        if (classGroup2Id.HasValue && 
            classSchedules.TryGetValue(classGroup2Id.Value, out var schedules2))
        {
            foreach (var schedule in schedules2)
            {
                if (schedule.DayOfWeek == dayOfWeekNormalized)
                {
                    var gapBefore = (schedule.StartTime - slot.EndTime).TotalMinutes;
                    var gapAfter = (slot.StartTime - schedule.EndTime).TotalMinutes;

                    if (gapBefore < minGapMinutes && gapAfter < minGapMinutes)
                    {
                        return true; // Conflict
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Check xem slot có conflict với các matches đã tạo không (cùng sân)
    /// </summary>
    private bool HasMatchConflict(
        ScheduleSlot slot,
        List<ActivityMatch> generatedMatches,
        TournamentScheduleRequest request)
    {
        var hasMultipleLocations = request.AvailableLocations != null && request.AvailableLocations.Count >= 2;

        foreach (var match in generatedMatches)
        {
            if (match.MatchDate == null || match.StartTime == null || match.EndTime == null)
                continue;

            if (slot.MatchDate.Date == match.MatchDate.Value.Date &&
                slot.StartTime < match.EndTime.Value && 
                slot.EndTime > match.StartTime.Value)
            {
                // Nếu có nhiều sân, chỉ conflict nếu cùng sân
                if (!hasMultipleLocations || slot.Location == match.Location)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Match Generation

    /// <summary>
    /// Generate matches cho Single Elimination tournament
    /// </summary>
    private List<ActivityMatch> GenerateSingleEliminationMatches(
        TournamentScheduleRequest request,
        List<ScheduleSlot> availableSlots,
        ClassAnalysis classAnalysis,
        Dictionary<int, List<ClassScheduleInfo>> classSchedules)
    {
        var matches = new List<ActivityMatch>();
        var teams = request.ClassGroupIds.ToList();
        var usedSlots = new HashSet<ScheduleSlot>();
        var matchNumber = 1;

        if (teams.Count < 2)
        {
            return matches;
        }

        // Tính số rounds
        var powerOfTwo = GetNextPowerOfTwo(teams.Count);
        var numberOfRounds = (int)Math.Ceiling(Math.Log2(powerOfTwo));
        var byeCount = powerOfTwo - teams.Count;

        // Shuffle teams
        var random = new Random();
        var shuffledTeams = teams.OrderBy(x => random.Next()).ToList();

        // Chọn teams được bye
        var byeTeams = new List<int>();
        for (int i = 0; i < byeCount && shuffledTeams.Any(); i++)
        {
            var byeIndex = random.Next(shuffledTeams.Count);
            byeTeams.Add(shuffledTeams[byeIndex]);
            shuffledTeams.RemoveAt(byeIndex);
        }

        // Tạo matches cho round 1
        // QUAN TRỌNG: Round 1 phải chọn slots sớm nhất có thể
        var round1Matches = new List<ActivityMatch>();
        var playCount = shuffledTeams.Count;
        var maxMatches = playCount / 2;

        // Sắp xếp availableSlots theo thời gian sớm nhất để round 1 chọn trước
        var sortedSlotsForRound1 = availableSlots
            .Where(s => !usedSlots.Contains(s))
            .OrderBy(s => s.MatchDate)
            .ThenBy(s => s.StartTime)
            .ToList();

        for (int i = 0; i < maxMatches && (i * 2 + 1) < shuffledTeams.Count; i++)
        {
            var team1 = shuffledTeams[i * 2];
            var team2 = shuffledTeams[i * 2 + 1];

            // Tìm slot tốt nhất từ danh sách đã sắp xếp
            var slot = FindBestSlot(
                sortedSlotsForRound1,
                usedSlots,
                team1,
                team2,
                classSchedules,
                classAnalysis,
                request,
                matches);

            if (slot == null)
            {
                break; // Không còn slot
            }

            usedSlots.Add(slot);

            var match = new ActivityMatch
            {
                ActivityId = request.ActivityId,
                SportId = request.SportId,
                ClassGroup1Id = team1,
                ClassGroup2Id = team2,
                Grade = request.Grade,
                MatchDate = slot.MatchDate,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                Location = slot.Location,
                Status = MatchStatus.Pending,
                Round = 1,
                RoundName = "Vòng 1",
                MatchNumber = matchNumber++,
                IsBye = false
            };

            round1Matches.Add(match);
            matches.Add(match);
        }

        // Tạo matches cho các rounds tiếp theo
        var currentRoundParticipants = new List<(int? TeamId, int? PreviousMatchId)>();
        
        // Thêm bye teams vào round 2
        foreach (var byeTeam in byeTeams)
        {
            currentRoundParticipants.Add((byeTeam, null));
        }

        // Thêm winners từ round 1
        foreach (var match in round1Matches)
        {
            currentRoundParticipants.Add((null, match.MatchNumber));
        }

        for (int round = 2; round <= numberOfRounds; round++)
        {
            var roundName = round == numberOfRounds ? "Chung kết" : $"Vòng {round}";
            var roundMatches = new List<ActivityMatch>();
            var nextRoundParticipants = new List<(int? TeamId, int? PreviousMatchId)>();

            for (int i = 0; i < currentRoundParticipants.Count / 2; i++)
            {
                var participant1 = currentRoundParticipants[i * 2];
                var participant2 = currentRoundParticipants[i * 2 + 1];

                // Lấy các lớp có thể tham gia match này
                var possibleTeams = GetPossibleTeams(participant1, participant2, matches);

                // Tìm slot tốt nhất
                var slot = FindBestSlotForRound(
                    availableSlots,
                    usedSlots,
                    possibleTeams,
                    classSchedules,
                    classAnalysis,
                    request,
                    matches,
                    round);

                if (slot == null)
                {
                    break; // Không còn slot
                }

                usedSlots.Add(slot);

                var match = new ActivityMatch
                {
                    ActivityId = request.ActivityId,
                    SportId = request.SportId,
                    ClassGroup1Id = participant1.TeamId,
                    ClassGroup2Id = participant2.TeamId,
                    Grade = request.Grade,
                    MatchDate = slot.MatchDate,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Location = slot.Location,
                    Status = MatchStatus.Pending,
                    Round = round,
                    RoundName = roundName,
                    MatchNumber = matchNumber++,
                    IsBye = participant1.TeamId.HasValue || participant2.TeamId.HasValue
                };

                // Link previous matches
                if (participant1.PreviousMatchId.HasValue)
                {
                    var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant1.PreviousMatchId.Value);
                    if (prevMatch != null)
                    {
                        prevMatch.NextMatchId = match.MatchNumber;
                    }
                }

                if (participant2.PreviousMatchId.HasValue)
                {
                    var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant2.PreviousMatchId.Value);
                    if (prevMatch != null)
                    {
                        prevMatch.NextMatchId = match.MatchNumber;
                    }
                }

                roundMatches.Add(match);
                matches.Add(match);
                nextRoundParticipants.Add((null, match.MatchNumber));
            }

            // Nếu có đội lẻ, thêm vào round tiếp theo
            if (currentRoundParticipants.Count % 2 == 1)
            {
                nextRoundParticipants.Add(currentRoundParticipants.Last());
            }

            currentRoundParticipants = nextRoundParticipants;
        }

        return matches;
    }

    /// <summary>
    /// Generate matches cho Round Robin tournament
    /// </summary>
    private List<ActivityMatch> GenerateRoundRobinMatches(
        TournamentScheduleRequest request,
        List<ScheduleSlot> availableSlots,
        ClassAnalysis classAnalysis,
        Dictionary<int, List<ClassScheduleInfo>> classSchedules)
    {
        var matches = new List<ActivityMatch>();
        var teams = request.ClassGroupIds.ToList();
        var usedSlots = new HashSet<ScheduleSlot>();
        var matchNumber = 1;

        // Round robin: mỗi team đấu với tất cả teams khác
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                var team1 = teams[i];
                var team2 = teams[j];

                var slot = FindBestSlot(
                    availableSlots,
                    usedSlots,
                    team1,
                    team2,
                    classSchedules,
                    classAnalysis,
                    request,
                    matches);

                if (slot == null)
                {
                    break; // Không còn slot
                }

                usedSlots.Add(slot);

                var match = new ActivityMatch
                {
                    ActivityId = request.ActivityId,
                    SportId = request.SportId,
                    ClassGroup1Id = team1,
                    ClassGroup2Id = team2,
                    Grade = request.Grade,
                    MatchDate = slot.MatchDate,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Location = slot.Location,
                    Status = MatchStatus.Pending,
                    Round = 1,
                    RoundName = "Vòng bảng",
                    MatchNumber = matchNumber++,
                    IsBye = false
                };

                matches.Add(match);
            }
        }

        return matches;
    }

    #endregion

    #region Slot Finding

    /// <summary>
    /// Tìm slot tốt nhất cho một match cụ thể
    /// </summary>
    private ScheduleSlot? FindBestSlot(
        List<ScheduleSlot> availableSlots,
        HashSet<ScheduleSlot> usedSlots,
        int team1,
        int team2,
        Dictionary<int, List<ClassScheduleInfo>> classSchedules,
        ClassAnalysis classAnalysis,
        TournamentScheduleRequest request,
        List<ActivityMatch> existingMatches)
    {
        var candidateSlots = availableSlots
            .Where(s => !usedSlots.Contains(s))
            .ToList();

        // Ưu tiên 1: Slot không conflict với lịch học
        var nonConflictSlots = candidateSlots
            .Where(s => !HasScheduleConflict(s, team1, team2, classSchedules, request.MinGapBetweenMatches))
            .Where(s => !HasMatchConflict(s, existingMatches, request))
            .ToList();

        if (nonConflictSlots.Any())
        {
            // Ưu tiên slot phù hợp với buổi học
            return SelectOptimalSlot(nonConflictSlots, team1, team2, classAnalysis, request);
        }

        // Fallback: Chọn slot đầu tiên còn lại (có thể conflict)
        return candidateSlots
            .Where(s => !HasMatchConflict(s, existingMatches, request))
            .FirstOrDefault();
    }

    /// <summary>
    /// Tìm slot tốt nhất cho round (có thể có nhiều lớp có thể tham gia)
    /// QUAN TRỌNG: Round sau phải diễn ra sau round trước
    /// </summary>
    private ScheduleSlot? FindBestSlotForRound(
        List<ScheduleSlot> availableSlots,
        HashSet<ScheduleSlot> usedSlots,
        HashSet<int> possibleTeams,
        Dictionary<int, List<ClassScheduleInfo>> classSchedules,
        ClassAnalysis classAnalysis,
        TournamentScheduleRequest request,
        List<ActivityMatch> existingMatches,
        int round)
    {
        // Tìm thời gian muộn nhất của round trước
        var latestPreviousRoundTime = existingMatches
            .Where(m => m.Round < round && m.MatchDate.HasValue && m.EndTime.HasValue)
            .Select(m => m.MatchDate.Value.Date.Add(m.EndTime.Value))
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();

        var candidateSlots = availableSlots
            .Where(s => !usedSlots.Contains(s))
            .Where(s =>
            {
                // QUAN TRỌNG: Round sau phải diễn ra sau round trước + minGapBetweenMatches
                if (latestPreviousRoundTime > DateTime.MinValue)
                {
                    var slotStartTime = s.MatchDate.Date.Add(s.StartTime);
                    var requiredStartTime = latestPreviousRoundTime.AddMinutes(request.MinGapBetweenMatches);
                    if (slotStartTime <= requiredStartTime)
                    {
                        return false; // Quá sớm, chưa đủ thời gian nghỉ sau round trước
                    }
                }
                return true;
            })
            .ToList();

        // Tìm slot không conflict với bất kỳ lớp nào có thể tham gia
        var nonConflictSlots = candidateSlots
            .Where(slot =>
            {
                // Check conflict với tất cả lớp có thể tham gia
                foreach (var teamId in possibleTeams)
                {
                    if (HasScheduleConflict(slot, teamId, null, classSchedules, request.MinGapBetweenMatches))
                    {
                        return false;
                    }
                }
                return !HasMatchConflict(slot, existingMatches, request);
            })
            .ToList();

        if (nonConflictSlots.Any())
        {
            return SelectOptimalSlotForRound(nonConflictSlots, possibleTeams, classAnalysis, request);
        }

        // Fallback: vẫn phải đảm bảo sau round trước
        return candidateSlots
            .Where(s => !HasMatchConflict(s, existingMatches, request))
            .FirstOrDefault();
    }

    /// <summary>
    /// Chọn slot tối ưu dựa trên buổi học
    /// </summary>
    private ScheduleSlot SelectOptimalSlot(
        List<ScheduleSlot> slots,
        int team1,
        int team2,
        ClassAnalysis classAnalysis,
        TournamentScheduleRequest request)
    {
        // Ưu tiên: Lớp sáng đấu buổi chiều, lớp chiều đấu buổi sáng
        var team1IsMorning = classAnalysis.MorningClasses.Contains(team1);
        var team2IsMorning = classAnalysis.MorningClasses.Contains(team2);

        return slots
            .OrderBy(s =>
            {
                var hour = s.StartTime.Hours;
                var isMorningSlot = hour >= 6 && hour < 12;
                var isAfternoonSlot = hour >= 12 && hour < 18;

                // Ưu tiên cao nhất: Cả 2 lớp sáng → đấu buổi chiều
                if (team1IsMorning && team2IsMorning && isAfternoonSlot)
                    return 0;

                // Ưu tiên cao: Cả 2 lớp chiều → đấu buổi sáng
                if (!team1IsMorning && !team2IsMorning && isMorningSlot)
                    return 1;

                // Ưu tiên trung bình: Khác buổi hoặc chưa xác định
                return 2;
            })
            .ThenBy(s => s.MatchDate)
            .ThenBy(s => s.StartTime)
            .First();
    }

    /// <summary>
    /// Chọn slot tối ưu cho round (nhiều lớp có thể tham gia)
    /// </summary>
    private ScheduleSlot SelectOptimalSlotForRound(
        List<ScheduleSlot> slots,
        HashSet<int> possibleTeams,
        ClassAnalysis classAnalysis,
        TournamentScheduleRequest request)
    {
        return slots
            .OrderBy(s => s.MatchDate)
            .ThenBy(s => s.StartTime)
            .First();
    }

    #endregion

    #region Helpers

    private int GetNextPowerOfTwo(int n)
    {
        if (n <= 0) return 1;
        if (n == 1) return 1;
        int power = 1;
        while (power < n)
        {
            power *= 2;
        }
        return power;
    }

    private int CalculateExpectedMatches(int numberOfTeams, string format)
    {
        return format.ToLower() switch
        {
            "singleelimination" => numberOfTeams - 1,
            "roundrobin" => numberOfTeams * (numberOfTeams - 1) / 2,
            "doubleelimination" => (numberOfTeams - 1) * 2,
            _ => numberOfTeams - 1
        };
    }

    private HashSet<int> GetPossibleTeams(
        (int? TeamId, int? PreviousMatchId) participant1,
        (int? TeamId, int? PreviousMatchId) participant2,
        List<ActivityMatch> matches)
    {
        var possibleTeams = new HashSet<int>();

        if (participant1.TeamId.HasValue)
        {
            possibleTeams.Add(participant1.TeamId.Value);
        }
        else if (participant1.PreviousMatchId.HasValue)
        {
            var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant1.PreviousMatchId.Value);
            if (prevMatch != null)
            {
                if (prevMatch.ClassGroup1Id.HasValue) possibleTeams.Add(prevMatch.ClassGroup1Id.Value);
                if (prevMatch.ClassGroup2Id.HasValue) possibleTeams.Add(prevMatch.ClassGroup2Id.Value);
            }
        }

        if (participant2.TeamId.HasValue)
        {
            possibleTeams.Add(participant2.TeamId.Value);
        }
        else if (participant2.PreviousMatchId.HasValue)
        {
            var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant2.PreviousMatchId.Value);
            if (prevMatch != null)
            {
                if (prevMatch.ClassGroup1Id.HasValue) possibleTeams.Add(prevMatch.ClassGroup1Id.Value);
                if (prevMatch.ClassGroup2Id.HasValue) possibleTeams.Add(prevMatch.ClassGroup2Id.Value);
            }
        }

        return possibleTeams;
    }

    private List<ActivityMatch> SortAndRenumberMatches(List<ActivityMatch> matches)
    {
        // QUAN TRỌNG: Sắp xếp theo Round trước, sau đó mới đến thời gian
        // Đảm bảo Round 1 → Round 2 → Round 3 (Chung kết)
        var sorted = matches
            .Where(m => m.MatchDate.HasValue && m.StartTime.HasValue)
            .OrderBy(m => m.Round) // Round trước
            .ThenBy(m => m.MatchDate) // Sau đó ngày
            .ThenBy(m => m.StartTime) // Sau đó giờ
            .ToList();

        // Đánh lại số
        var oldToNewMap = new Dictionary<int, int>();
        for (int i = 0; i < sorted.Count; i++)
        {
            oldToNewMap[sorted[i].MatchNumber] = i + 1;
            sorted[i].MatchNumber = i + 1;
        }

        // Cập nhật NextMatchId
        foreach (var match in sorted)
        {
            if (match.NextMatchId.HasValue && oldToNewMap.TryGetValue(match.NextMatchId.Value, out var newNextMatchId))
            {
                match.NextMatchId = newNextMatchId;
            }
        }

        return sorted;
    }

    private List<SlotWarningReason> GenerateWarnings(
        int availableSlotsCount,
        TournamentScheduleRequest request,
        int existingMatchesCount)
    {
        var warnings = new List<SlotWarningReason>();

        var expectedMatches = CalculateExpectedMatches(request.ClassGroupIds.Count, request.TournamentFormat);
        var minSlotsNeeded = expectedMatches;

        if (availableSlotsCount < minSlotsNeeded)
        {
            warnings.Add(new SlotWarningReason
            {
                WarningType = "InsufficientSlots",
                Title = "Không đủ slots",
                Description = $"Chỉ có {availableSlotsCount} slots, cần ít nhất {minSlotsNeeded} slots.",
                CurrentValue = $"{availableSlotsCount} slots",
                RecommendedValue = $"Ít nhất {minSlotsNeeded} slots",
                Solution = "Mở rộng khoảng thời gian hoặc thêm sân thi đấu",
                Severity = "Critical",
                Field = "startDate, endDate, availableLocations"
            });
        }

        if (existingMatchesCount > 0)
        {
            warnings.Add(new SlotWarningReason
            {
                WarningType = "ExistingMatches",
                Title = "Có trận đấu đã được lên lịch",
                Description = $"Có {existingMatchesCount} trận đấu khác đã được lên lịch, có thể ảnh hưởng đến slots available.",
                CurrentValue = $"{existingMatchesCount} trận",
                RecommendedValue = "0 trận",
                Solution = "Kiểm tra và điều chỉnh thời gian các trận đấu đã có",
                Severity = "Medium",
                Field = "startDate, endDate"
            });
        }

        return warnings;
    }

    #endregion

    #region Helper Classes

    private class ClassScheduleInfo
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Subject { get; set; }
    }

    private class ClassAnalysis
    {
        public HashSet<int> MorningClasses { get; set; } = new();
        public HashSet<int> AfternoonClasses { get; set; } = new();
    }

    #endregion
}

