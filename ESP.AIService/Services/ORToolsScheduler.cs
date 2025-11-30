using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.LinearSolver;
using ESP.AIService.Models;
using EduShpere.Infrastructure;
using EduShpere.Domain.Models;
using Microsoft.EntityFrameworkCore;
using AiActivityMatch = ESP.AIService.Entities.ActivityMatch;
using AiMatchStatus = ESP.AIService.Entities.MatchStatus;
using EduShpere.Infrastructure.Repositories;

namespace ESP.AIService.Services;

public class ORToolsScheduler
{
    /// <summary>
    /// Tối ưu hóa lịch thi đấu với OR-Tools
    /// </summary>
    public TournamentScheduleResponse OptimizeSchedule(
        TournamentScheduleRequest request,
        List<ScheduleSlot> slots,
        EduShpereDbContext dbContext)
    {
        var response = new TournamentScheduleResponse
        {
            Success = false,
            IsOptimal = false,
            Explanation = string.Empty
        };

        if (!slots.Any())
        {
            response.Explanation = "Không có slots available.";
            return response;
        }

        // Lấy các matches đã có để check conflict
        List<AiActivityMatch> existingMatches = new();
        
        try
        {
            // Query trực tiếp từ table ActivityMatches bằng raw SQL
            // Chỉ select các columns thực sự có trong database
            // Cast các cột để đảm bảo kiểu dữ liệu đúng (tránh lỗi byte -> int)
            var sql = @"
                SELECT CAST(Id AS INT) AS Id, 
                       CAST(ActivityId AS INT) AS ActivityId, 
                       CAST(SportId AS INT) AS SportId, 
                       CAST(ClassGroup1Id AS INT) AS ClassGroup1Id, 
                       CAST(ClassGroup2Id AS INT) AS ClassGroup2Id, 
                       CAST(Grade AS INT) AS Grade, 
                       MatchDate, StartTime, EndTime, Location, 
                       CAST(Status AS INT) AS Status, 
                       CAST(Score1 AS INT) AS Score1, 
                       CAST(Score2 AS INT) AS Score2,
                       CAST(WinnerClassGroupId AS INT) AS WinnerClassGroupId, 
                       CAST(Round AS INT) AS Round, 
                       RoundName, 
                       CAST(MatchNumber AS INT) AS MatchNumber, 
                       CAST(NextMatchId AS INT) AS NextMatchId, 
                       CAST(IsBye AS BIT) AS IsBye, 
                       Notes, CreatedAt, 
                       CAST(CreatedBy AS INT) AS CreatedBy, 
                       UpdatedAt, 
                       CAST(UpdatedBy AS INT) AS UpdatedBy, 
                       CAST(IsDeleted AS BIT) AS IsDeleted
                FROM ActivityMatches 
                WHERE ActivityId = {0} 
                  AND MatchDate IS NOT NULL 
                  AND StartTime IS NOT NULL 
                  AND EndTime IS NOT NULL 
                  AND Status != {1}
                  AND IsDeleted = 0";
            
            var matchDtos = dbContext.Database
                .SqlQueryRaw<Models.ActivityMatchDto>(sql, request.ActivityId, (int)AiMatchStatus.Cancelled)
                .ToList();
            
            existingMatches = matchDtos.Select(dto => dto.ToActivityMatch()).ToList();
        }
        catch (Exception ex)
        {
            // Nếu không có table hoặc có lỗi
            Console.WriteLine($"⚠️ Không thể query ActivityMatches: {ex.Message}");
            Console.WriteLine("   Table có thể chưa được tạo. Sẽ không check conflict với matches cũ.");
            existingMatches = new List<AiActivityMatch>();
        }

        // Filter slots: loại bỏ các slots conflict
        Console.WriteLine($"🔍 Đang filter slots (tổng: {slots.Count})...");
        var availableSlots = FilterAvailableSlots(slots, existingMatches, request, dbContext);
        Console.WriteLine($"   Slots available sau khi filter: {availableSlots.Count}");

        if (!availableSlots.Any())
        {
            response.Explanation = "Không có slots nào available sau khi filter conflicts (có thể do trùng với lịch học hoặc matches đã có).";
            return response;
        }

        // Tính số matches cần tạo dựa trên tournament format
        var numberOfMatches = CalculateNumberOfMatches(request.ClassGroupIds.Count, request.TournamentFormat);
        var numberOfRounds = CalculateNumberOfRounds(request.ClassGroupIds.Count, request.TournamentFormat);

        // Tạo solver
        var solver = Solver.CreateSolver("SCIP");
        if (solver == null)
        {
            response.Explanation = "Không thể tạo OR-Tools solver.";
            return response;
        }

        // Variables: x[i] = 1 nếu slot i được chọn, 0 nếu không
        var variables = new Variable[availableSlots.Count];
        for (int i = 0; i < availableSlots.Count; i++)
        {
            variables[i] = solver.MakeIntVar(0, 1, $"slot_{i}");
        }

        // Constraint 1: Chọn đúng số matches cần thiết
        var matchConstraint = solver.MakeConstraint(numberOfMatches, numberOfMatches, "exact_matches");
        for (int i = 0; i < availableSlots.Count; i++)
        {
            matchConstraint.SetCoefficient(variables[i], 1);
        }

        // Constraint 2: Tối đa số matches mỗi ngày
        var slotsByDate = availableSlots.GroupBy(s => s.MatchDate.Date).ToList();
        foreach (var dateGroup in slotsByDate)
        {
            var dateConstraint = solver.MakeConstraint(0, request.MaxMatchesPerDay, $"max_per_day_{dateGroup.Key:yyyyMMdd}");
            var indices = dateGroup.Select((s, idx) => availableSlots.IndexOf(s)).Where(idx => idx >= 0).ToList();
            foreach (var idx in indices)
            {
                dateConstraint.SetCoefficient(variables[idx], 1);
            }
        }

        // Constraint 3: Không overlap thời gian (mỗi slot chỉ có thể chọn 1 match)
        // Đã được xử lý bằng cách filter slots trước

        // Objective: Tối đa hóa tổng ML score
        var objective = solver.Objective();
        for (int i = 0; i < availableSlots.Count; i++)
        {
            objective.SetCoefficient(variables[i], (double)availableSlots[i].MLScore);
        }
        objective.SetMaximization();

        // Solve
        var resultStatus = solver.Solve();

        if (resultStatus != Solver.ResultStatus.OPTIMAL && resultStatus != Solver.ResultStatus.FEASIBLE)
        {
            response.Explanation = $"Không tìm được solution tối ưu. Status: {resultStatus}";
            return response;
        }

        // Tạo matches từ solution
        var selectedSlots = new List<ScheduleSlot>();
        for (int i = 0; i < availableSlots.Count; i++)
        {
            if (variables[i].SolutionValue() > 0.5) // > 0.5 nghĩa là được chọn
            {
                selectedSlots.Add(availableSlots[i]);
            }
        }

        // Sắp xếp slots theo thời gian
        selectedSlots = selectedSlots.OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).ToList();

        // Tạo matches
        var matches = GenerateMatches(request, selectedSlots, numberOfRounds);

        response.Success = true;
        response.IsOptimal = resultStatus == Solver.ResultStatus.OPTIMAL;
        response.GeneratedMatches = matches;
        response.ObjectiveValue = (float)solver.Objective().Value();
        response.TotalMatches = matches.Count;
        response.TotalRounds = numberOfRounds;
        response.Explanation = $"Đã tạo {matches.Count} matches trong {numberOfRounds} rounds. " +
                              $"Objective value: {response.ObjectiveValue:F2}";

        return response;
    }

    private List<ScheduleSlot> FilterAvailableSlots(
        List<ScheduleSlot> slots,
        List<AiActivityMatch> existingMatches,
        TournamentScheduleRequest request,
        EduShpereDbContext dbContext)
    {
        var availableSlots = new List<ScheduleSlot>();

        // Lấy lịch học của các lớp tham gia
        var classGroupSchedules = GetClassGroupSchedules(request.ClassGroupIds, dbContext);
        Console.WriteLine($"   📚 Đã load {classGroupSchedules.Count} lịch học từ {request.ClassGroupIds.Count} lớp tham gia");

        // Lấy tất cả participants của các lớp tham gia để check conflict với activities khác
        var participantsInClassGroups = GetParticipantsInClassGroups(request.ClassGroupIds, dbContext);
        Console.WriteLine($"   👥 Đã load {participantsInClassGroups.Count} participants từ {request.ClassGroupIds.Count} lớp tham gia");
        
        // Lấy tất cả activities khác mà participants đã tham gia (để check conflict)
        var participantActivityConflicts = GetParticipantActivityConflicts(
            participantsInClassGroups, 
            request.ActivityId, 
            request.StartDate, 
            request.EndDate, 
            dbContext);
        Console.WriteLine($"   ⚠️ Đã phát hiện {participantActivityConflicts.Count} activities khác có thể conflict với participants");

        // Tạo dictionary để lookup nhanh lịch học theo ClassGroupId
        var schedulesByClassGroup = classGroupSchedules
            .GroupBy(s => s.ClassGroupId)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        // Phân tích lịch học để tối ưu hóa: xác định lớp nào học buổi sáng/chiều
        var classGroupScheduleAnalysis = AnalyzeClassGroupSchedules(classGroupSchedules, request.ClassGroupIds);
        Console.WriteLine($"   📊 Phân tích lịch học: {classGroupScheduleAnalysis.MorningClasses.Count} lớp buổi sáng, {classGroupScheduleAnalysis.AfternoonClasses.Count} lớp buổi chiều");

        int conflictWithMatches = 0;
        int conflictWithSchedules = 0;
        int conflictWithLocation = 0;
        int conflictWithParticipantActivities = 0;

        foreach (var slot in slots)
        {
            bool isAvailable = true;

            // Check conflict với matches đã có
            foreach (var existingMatch in existingMatches)
            {
                if (existingMatch.MatchDate.HasValue && 
                    existingMatch.StartTime.HasValue && 
                    existingMatch.EndTime.HasValue)
                {
                    var existingDate = existingMatch.MatchDate.Value.Date;
                    var existingStart = existingMatch.StartTime.Value;
                    var existingEnd = existingMatch.EndTime.Value;

                    // Check cùng ngày và overlap thời gian
                    if (slot.MatchDate.Date == existingDate)
                    {
                        if (slot.StartTime < existingEnd && slot.EndTime > existingStart)
                        {
                            // Check cùng location
                            if (string.IsNullOrEmpty(slot.Location) || 
                                string.IsNullOrEmpty(existingMatch.Location) ||
                                slot.Location == existingMatch.Location)
                            {
                                isAvailable = false;
                                conflictWithMatches++;
                                break;
                            }
                        }
                    }
                }
            }

            // Check conflict với lịch học của các lớp
            // Lưu ý: Khi filter slots, chúng ta chưa biết match nào sẽ được gán cho slot nào
            // Nên cách an toàn nhất là check lịch học của TẤT CẢ các lớp tham gia tournament
            // Điều này đảm bảo slot không conflict với bất kỳ lớp nào, dù lớp đó có tham gia match hay không
            // Cách này hơi strict nhưng an toàn, đảm bảo không có lớp nào bị ảnh hưởng
            if (isAvailable && schedulesByClassGroup.Any())
            {
                var dayOfWeek = (int)slot.MatchDate.DayOfWeek; // 0 = Sunday, 1 = Monday, ...
                // Convert to 1-7 format (1 = Monday, 7 = Sunday)
                var dayOfWeekNormalized = dayOfWeek == 0 ? 7 : dayOfWeek;

                // Check lịch học của tất cả các lớp tham gia
                foreach (var classGroupId in request.ClassGroupIds)
                {
                    if (schedulesByClassGroup.TryGetValue(classGroupId, out var schedules))
                    {
                        foreach (var schedule in schedules)
                        {
                            // Check nếu slot trùng với giờ học của lớp
                            if (schedule.DayOfWeek == dayOfWeekNormalized)
                            {
                                // Check overlap thời gian
                                if (slot.StartTime < schedule.EndTime && slot.EndTime > schedule.StartTime)
                                {
                                    isAvailable = false;
                                    conflictWithSchedules++;
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (!isAvailable) break;
                }
            }
            
            // Check conflict với các activities khác của participants
            // Một học sinh không được có hai activity tại cùng thời điểm
            if (isAvailable && participantActivityConflicts.Any())
            {
                var slotDateTime = slot.MatchDate.Date.Add(slot.StartTime);
                var slotEndDateTime = slot.MatchDate.Date.Add(slot.EndTime);
                
                foreach (var conflict in participantActivityConflicts)
                {
                    // Check overlap thời gian với activity khác
                    if (conflict.StartDate.HasValue && conflict.EndDate.HasValue)
                    {
                        var conflictStart = conflict.StartDate.Value;
                        var conflictEnd = conflict.EndDate.Value;
                        
                        // Check overlap
                        if (slotDateTime < conflictEnd && slotEndDateTime > conflictStart)
                        {
                            // Có conflict - kiểm tra xem có participant nào trong conflict này thuộc các lớp tham gia không
                            var hasParticipantConflict = participantsInClassGroups.Any(p => 
                                conflict.ParticipantIds.Contains(p.UserId));
                            
                            if (hasParticipantConflict)
                            {
                                isAvailable = false;
                                conflictWithParticipantActivities++;
                                break;
                            }
                        }
                    }
                }
            }
            
            // Tăng ML Score dựa trên heuristic: ưu tiên slot không conflict với lịch học
            // Nếu slot nằm trong giờ trống của các lớp tham gia → tăng score
            if (isAvailable && classGroupScheduleAnalysis != null)
            {
                var slotHour = slot.StartTime.Hours;
                var isMorningSlot = slotHour >= 6 && slotHour < 12;
                var isAfternoonSlot = slotHour >= 12 && slotHour < 18;
                var isWeekend = slot.MatchDate.DayOfWeek == DayOfWeek.Saturday || slot.MatchDate.DayOfWeek == DayOfWeek.Sunday;
                
                float heuristicBonus = 0.0f;
                
                // Heuristic 1: Ưu tiên xếp các lớp có lịch học buổi sáng đấu với nhau vào buổi chiều
                // và ngược lại (lớp buổi chiều đấu với nhau vào buổi sáng)
                if (isAfternoonSlot && classGroupScheduleAnalysis.MorningClasses.Count >= 2)
                {
                    heuristicBonus += 5.0f; // Tăng điểm cao cho việc xếp lớp buổi sáng đấu buổi chiều
                }
                
                if (isMorningSlot && classGroupScheduleAnalysis.AfternoonClasses.Count >= 2)
                {
                    heuristicBonus += 5.0f; // Tăng điểm cao cho việc xếp lớp buổi chiều đấu buổi sáng
                }
                
                // Heuristic 2: Ưu tiên slot mà nhiều lớp đều trống cùng lúc
                // Check xem slot này có phù hợp với tất cả các lớp không
                bool slotFitsAllTimetables = true;
                int classesWithFreeSlot = 0;
                var dayOfWeek = (int)slot.MatchDate.DayOfWeek;
                var dayOfWeekNormalized = dayOfWeek == 0 ? 7 : dayOfWeek;
                
                foreach (var classGroupId in request.ClassGroupIds)
                {
                    bool classHasFreeSlot = true;
                    if (schedulesByClassGroup.TryGetValue(classGroupId, out var schedules))
                    {
                        foreach (var schedule in schedules)
                        {
                            if (schedule.DayOfWeek == dayOfWeekNormalized)
                            {
                                if (slot.StartTime < schedule.EndTime && slot.EndTime > schedule.StartTime)
                                {
                                    classHasFreeSlot = false;
                                    slotFitsAllTimetables = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (classHasFreeSlot)
                    {
                        classesWithFreeSlot++;
                    }
                }
                
                // Nếu slot phù hợp với tất cả các lớp → tăng điểm cao
                if (slotFitsAllTimetables && request.ClassGroupIds.Count >= 2)
                {
                    heuristicBonus += 10.0f;
                }
                // Nếu slot phù hợp với nhiều lớp → tăng điểm vừa
                else if (classesWithFreeSlot >= request.ClassGroupIds.Count * 0.8) // 80% lớp trống
                {
                    heuristicBonus += 7.0f;
                }
                
                // Heuristic 3: Ưu tiên lớp có cùng lịch học đấu với nhau
                // Check xem có cặp lớp nào có lịch học tương đồng không
                var similarTimetablePairs = 0;
                for (int i = 0; i < request.ClassGroupIds.Count; i++)
                {
                    for (int j = i + 1; j < request.ClassGroupIds.Count; j++)
                    {
                        var classA = request.ClassGroupIds[i];
                        var classB = request.ClassGroupIds[j];
                        
                        if (ClassesHaveSimilarTimetable(classA, classB, schedulesByClassGroup, dayOfWeekNormalized))
                        {
                            similarTimetablePairs++;
                        }
                    }
                }
                
                // Nếu có nhiều cặp lớp có lịch học tương đồng → tăng điểm
                if (similarTimetablePairs > 0)
                {
                    heuristicBonus += similarTimetablePairs * 2.0f;
                }
                
                // Heuristic 4: Không ưu tiên cuối tuần (score = 0, không cộng điểm)
                if (isWeekend)
                {
                    heuristicBonus += 0.0f; // Không cộng điểm cho cuối tuần
                }
                else
                {
                    // Ngày thường có thể có bonus nhỏ nếu phù hợp
                    heuristicBonus += 1.0f;
                }
                
                // Penalty cho conflicts (đã được xử lý ở trên, nhưng có thể thêm penalty nhỏ)
                // Nếu có conflict → score đã bị set isAvailable = false ở trên
                
                // Áp dụng heuristic bonus vào ML Score
                slot.MLScore = Math.Min(1.0f, slot.MLScore + (heuristicBonus / 100.0f)); // Normalize về 0-1
            }

            // Check location có trong danh sách available không
            if (isAvailable && request.AvailableLocations.Any())
            {
                if (!string.IsNullOrEmpty(slot.Location) && 
                    !request.AvailableLocations.Contains(slot.Location))
                {
                    isAvailable = false;
                    conflictWithLocation++;
                }
            }

            if (isAvailable)
            {
                slot.IsAvailable = true;
                availableSlots.Add(slot);
            }
        }

        Console.WriteLine($"   ⚠️ Conflicts: {conflictWithMatches} với matches cũ, {conflictWithSchedules} với lịch học, {conflictWithLocation} với location, {conflictWithParticipantActivities} với activities khác của participants");
        Console.WriteLine($"   ✅ Slots available: {availableSlots.Count}/{slots.Count}");

        return availableSlots;
    }
    
    /// <summary>
    /// Lấy danh sách participants của các lớp tham gia
    /// </summary>
    private List<ParticipantInfo> GetParticipantsInClassGroups(List<int> classGroupIds, EduShpereDbContext dbContext)
    {
        try
        {
            var participants = dbContext.Set<EduShpere.Domain.Models.ActivityParticipant>()
                .Where(ap => classGroupIds.Contains(ap.ClassGroupId ?? 0) && !ap.IsDeleted)
                .Select(ap => new ParticipantInfo
                {
                    UserId = ap.UserId,
                    ClassGroupId = ap.ClassGroupId ?? 0
                })
                .Distinct()
                .ToList();
            
            return participants;
        }
        catch
        {
            return new List<ParticipantInfo>();
        }
    }
    
    /// <summary>
    /// Lấy danh sách activities khác mà participants đã tham gia (để check conflict)
    /// </summary>
    private List<ParticipantActivityConflict> GetParticipantActivityConflicts(
        List<ParticipantInfo> participants,
        int currentActivityId,
        DateTime startDate,
        DateTime endDate,
        EduShpereDbContext dbContext)
    {
        try
        {
            var participantIds = participants.Select(p => p.UserId).ToList();
            if (!participantIds.Any())
                return new List<ParticipantActivityConflict>();
            
            // Lấy tất cả activities khác (không phải activity hiện tại) mà participants đã tham gia
            // và có thời gian overlap với khoảng thời gian của tournament
            var conflicts = dbContext.Set<EduShpere.Domain.Models.Activity>()
                .Where(a => a.Id != currentActivityId && 
                           !a.IsDeleted &&
                           a.StartDate.HasValue && 
                           a.EndDate.HasValue &&
                           a.StartDate.Value <= endDate &&
                           a.EndDate.Value >= startDate)
                .Select(a => new
                {
                    ActivityId = a.Id,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Participants = a.ActivityParticipants
                        .Where(ap => participantIds.Contains(ap.UserId) && !ap.IsDeleted)
                        .Select(ap => ap.UserId)
                        .ToList()
                })
                .Where(x => x.Participants.Any())
                .ToList()
                .Select(x => new ParticipantActivityConflict
                {
                    ActivityId = x.ActivityId,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ParticipantIds = x.Participants.ToHashSet()
                })
                .ToList();
            
            return conflicts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Lỗi khi lấy participant activity conflicts: {ex.Message}");
            return new List<ParticipantActivityConflict>();
        }
    }
    
    /// <summary>
    /// Phân tích lịch học để xác định lớp nào học buổi sáng/chiều
    /// </summary>
    private ClassGroupScheduleAnalysis AnalyzeClassGroupSchedules(
        List<ClassGroupSchedule> schedules,
        List<int> classGroupIds)
    {
        var analysis = new ClassGroupScheduleAnalysis
        {
            MorningClasses = new HashSet<int>(),
            AfternoonClasses = new HashSet<int>()
        };
        
        // Phân loại lớp theo giờ học chính
        // Buổi sáng: 6h - 12h
        // Buổi chiều: 12h - 18h
        var classGroupScheduleTimes = schedules
            .GroupBy(s => s.ClassGroupId)
            .ToDictionary(g => g.Key, g => g.Select(s => new { s.StartTime, s.EndTime }).ToList());
        
        foreach (var classGroupId in classGroupIds)
        {
            if (classGroupScheduleTimes.TryGetValue(classGroupId, out var times))
            {
                bool hasMorningClass = false;
                bool hasAfternoonClass = false;
                
                foreach (var time in times)
                {
                    var startHour = time.StartTime.Hours;
                    var endHour = time.EndTime.Hours;
                    
                    // Nếu có lớp học trong khoảng 6h-12h → buổi sáng
                    if (startHour >= 6 && startHour < 12)
                    {
                        hasMorningClass = true;
                    }
                    
                    // Nếu có lớp học trong khoảng 12h-18h → buổi chiều
                    if (startHour >= 12 && startHour < 18)
                    {
                        hasAfternoonClass = true;
                    }
                }
                
                // Phân loại: nếu chủ yếu học buổi sáng → morning class
                // Nếu chủ yếu học buổi chiều → afternoon class
                if (hasMorningClass && !hasAfternoonClass)
                {
                    analysis.MorningClasses.Add(classGroupId);
                }
                else if (hasAfternoonClass && !hasMorningClass)
                {
                    analysis.AfternoonClasses.Add(classGroupId);
                }
                else if (hasMorningClass && hasAfternoonClass)
                {
                    // Có cả sáng và chiều → đếm số tiết
                    var morningCount = times.Count(t => t.StartTime.Hours >= 6 && t.StartTime.Hours < 12);
                    var afternoonCount = times.Count(t => t.StartTime.Hours >= 12 && t.StartTime.Hours < 18);
                    
                    if (morningCount > afternoonCount)
                    {
                        analysis.MorningClasses.Add(classGroupId);
                    }
                    else
                    {
                        analysis.AfternoonClasses.Add(classGroupId);
                    }
                }
            }
        }
        
        return analysis;
    }

    private List<ClassGroupSchedule> GetClassGroupSchedules(List<int> classGroupIds, EduShpereDbContext dbContext)
    {
        try
        {
            // Lấy lịch học từ ClassGroupSchedule (lịch học khi tạo lớp)
            var schedules = dbContext.Set<ClassGroupSchedule>()
                .Where(s => classGroupIds.Contains(s.ClassGroupId) && !s.IsDeleted)
                .ToList();
            
            // Lấy lịch học từ Timetable (lịch học import từ CSV/Excel/ICS)
            try
            {
                var timetables = dbContext.Set<Timetable>()
                    .Where(t => classGroupIds.Contains(t.ClassGroupId) && !t.IsDeleted)
                    .ToList();
                
                // Convert Timetable sang ClassGroupSchedule format để sử dụng chung logic
                var timetableSchedules = timetables.Select(t => new ClassGroupSchedule
                {
                    Id = t.Id,
                    ClassGroupId = t.ClassGroupId,
                    DayOfWeek = t.DayOfWeek,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    Subject = t.SubjectName,
                    Period = 0 // Timetable không có Period, set 0
                }).ToList();
                
                // Merge cả hai danh sách
                schedules.AddRange(timetableSchedules);
            }
            catch
            {
                // Nếu bảng Timetable chưa tồn tại, chỉ dùng ClassGroupSchedule
                Console.WriteLine("   ⚠️ Bảng Timetables chưa tồn tại, chỉ sử dụng ClassGroupSchedule");
            }
            
            return schedules;
        }
        catch
        {
            // Nếu table chưa tồn tại hoặc có lỗi
            return new List<ClassGroupSchedule>();
        }
    }

    private int CalculateNumberOfMatches(int numberOfTeams, string format)
    {
        return format.ToLower() switch
        {
            "singleelimination" => numberOfTeams - 1, // Single elimination: n-1 matches
            "roundrobin" => numberOfTeams * (numberOfTeams - 1) / 2, // Round robin: n*(n-1)/2
            "doubleelimination" => (numberOfTeams - 1) * 2, // Double elimination: 2*(n-1)
            _ => numberOfTeams - 1 // Default: single elimination
        };
    }

    private int CalculateNumberOfRounds(int numberOfTeams, string format)
    {
        return format.ToLower() switch
        {
            "singleelimination" => (int)Math.Ceiling(Math.Log2(numberOfTeams)), // log2(n)
            "roundrobin" => numberOfTeams - 1, // Round robin: n-1 rounds
            "doubleelimination" => (int)Math.Ceiling(Math.Log2(numberOfTeams)) * 2, // Double: 2*log2(n)
            _ => (int)Math.Ceiling(Math.Log2(numberOfTeams)) // Default: single elimination
        };
    }

    private List<AiActivityMatch> GenerateMatches(
        TournamentScheduleRequest request,
        List<ScheduleSlot> slots,
        int numberOfRounds)
    {
        var matches = new List<AiActivityMatch>();
        var classGroupIds = request.ClassGroupIds.ToList();
        var matchNumber = 1;

        // Single Elimination Tournament
        if (request.TournamentFormat.ToLower() == "singleelimination")
        {
            matches = GenerateSingleEliminationMatches(request, slots, classGroupIds, numberOfRounds, ref matchNumber);
        }
        // Round Robin Tournament
        else if (request.TournamentFormat.ToLower() == "roundrobin")
        {
            matches = GenerateRoundRobinMatches(request, slots, classGroupIds, ref matchNumber);
        }
        // Default: Single Elimination
        else
        {
            matches = GenerateSingleEliminationMatches(request, slots, classGroupIds, numberOfRounds, ref matchNumber);
        }

        return matches;
    }

    private List<AiActivityMatch> GenerateSingleEliminationMatches(
        TournamentScheduleRequest request,
        List<ScheduleSlot> slots,
        List<int> classGroupIds,
        int numberOfRounds,
        ref int matchNumber)
    {
        var matches = new List<AiActivityMatch>();
        var slotIndex = 0;

        // Shuffle teams để random đội được bye nếu lẻ
        var shuffledTeams = classGroupIds.OrderBy(x => Guid.NewGuid()).ToList();
        int? byeTeam = null;

        // Nếu số đội lẻ, random chọn 1 đội được bye
        if (shuffledTeams.Count % 2 == 1)
        {
            var random = new Random();
            var byeIndex = random.Next(shuffledTeams.Count);
            byeTeam = shuffledTeams[byeIndex];
            shuffledTeams.RemoveAt(byeIndex);
        }

        // Tính số matches cần cho mỗi round (Single Elimination)
        var matchesPerRound = new List<int>();
        var teamsCount = shuffledTeams.Count + (byeTeam.HasValue ? 1 : 0);
        var remainingTeams = teamsCount;
        var hasByeInRound = new Dictionary<int, bool>(); // Track bye team trong mỗi round
        
        for (int r = 1; r <= numberOfRounds; r++)
        {
            var matchesInRound = remainingTeams / 2;
            var hasBye = remainingTeams % 2 == 1;
            matchesPerRound.Add(matchesInRound);
            hasByeInRound[r] = hasBye;
            
            // Teams còn lại sau round này = số matches + bye (nếu có)
            remainingTeams = matchesInRound + (hasBye ? 1 : 0);
        }

        // Dictionary để lưu matches theo round
        var matchesByRound = new Dictionary<int, List<AiActivityMatch>>();

        // Tạo matches cho tất cả các rounds (bao gồm cả chung kết)
        for (int round = 1; round <= numberOfRounds; round++)
        {
            var roundName = round == numberOfRounds ? "Chung kết" : 
                           round == numberOfRounds - 1 ? "Bán kết" : 
                           $"Vòng {round}";
            
            var roundMatches = new List<AiActivityMatch>();
            var matchesInThisRound = matchesPerRound[round - 1];

            if (round == 1)
            {
                // Round 1: Tạo matches từ shuffled teams (không bao gồm bye team)
                for (int m = 0; m < matchesInThisRound && slotIndex < slots.Count; m++)
                {
                    var slot = slots[slotIndex++];
                    var teamIndex = m * 2;
                    int? team1 = null;
                    int? team2 = null;

                    if (teamIndex < shuffledTeams.Count)
                    {
                        team1 = shuffledTeams[teamIndex];
                    }
                    if (teamIndex + 1 < shuffledTeams.Count)
                    {
                        team2 = shuffledTeams[teamIndex + 1];
                    }

                    var match = new AiActivityMatch
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
                        Status = AiMatchStatus.Pending,
                        Round = round,
                        RoundName = roundName,
                        MatchNumber = matchNumber++,
                        IsBye = false,
                        Notes = null
                    };

                    roundMatches.Add(match);
                    matches.Add(match);
                }
            }
            else if (round == 2 && byeTeam.HasValue)
            {
                // Round 2: Nếu có bye team từ round 1, match đầu tiên sẽ có bye team
                for (int m = 0; m < matchesInThisRound && slotIndex < slots.Count; m++)
                {
                    var slot = slots[slotIndex++];
                    var prevRoundName = round - 1 == numberOfRounds - 1 ? "Bán kết" : 
                                       round - 1 == numberOfRounds ? "Chung kết" : 
                                       $"Vòng {round - 1}";
                    
                    int? team1 = null;
                    int? team2 = null;
                    string notes = null;
                    bool isByeMatch = false;

                    if (m == 0 && byeTeam.HasValue)
                    {
                        // Match đầu tiên của round 2: bye team vs winner từ match đầu tiên của round 1
                        team1 = byeTeam.Value;
                        team2 = null; // Sẽ được cập nhật từ winner của match đầu tiên round 1
                        notes = $"Đội được bye vs Thắng trận Vòng 1";
                        isByeMatch = true;
                    }
                    else
                    {
                        notes = $"Chờ kết quả {prevRoundName}";
                    }

                    var match = new AiActivityMatch
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
                        Status = AiMatchStatus.Pending,
                        Round = round,
                        RoundName = roundName,
                        MatchNumber = matchNumber++,
                        IsBye = isByeMatch,
                        Notes = notes
                    };

                    roundMatches.Add(match);
                    matches.Add(match);
                }
            }
            else
            {
                // Round 2 trở đi (không có bye) hoặc round 3+: Teams sẽ được xác định từ winners
                for (int m = 0; m < matchesInThisRound && slotIndex < slots.Count; m++)
                {
                    var slot = slots[slotIndex++];
                    var prevRoundName = round - 1 == numberOfRounds - 1 ? "Bán kết" : 
                                       round - 1 == numberOfRounds ? "Chung kết" : 
                                       $"Vòng {round - 1}";
                    
                    var match = new AiActivityMatch
                    {
                        ActivityId = request.ActivityId,
                        SportId = request.SportId,
                        ClassGroup1Id = null,
                        ClassGroup2Id = null,
                        Grade = request.Grade,
                        MatchDate = slot.MatchDate,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Location = slot.Location,
                        Status = AiMatchStatus.Pending,
                        Round = round,
                        RoundName = roundName,
                        MatchNumber = matchNumber++,
                        IsBye = false,
                        Notes = $"Chờ kết quả {prevRoundName}"
                    };

                    roundMatches.Add(match);
                    matches.Add(match);
                }
            }

            matchesByRound[round] = roundMatches;
        }

        // Link NextMatchId: Mỗi match trong round N link đến match trong round N+1
        for (int round = 1; round < numberOfRounds; round++)
        {
            if (!matchesByRound.ContainsKey(round) || !matchesByRound.ContainsKey(round + 1))
                continue;
                
            var currentRoundMatches = matchesByRound[round];
            var nextRoundMatches = matchesByRound[round + 1];

            if (round == 1 && byeTeam.HasValue)
            {
                // Xử lý đặc biệt khi có bye team
                // Match đầu tiên của round 1 -> Match đầu tiên của round 2 (nơi có bye team)
                if (currentRoundMatches.Count > 0 && nextRoundMatches.Count > 0)
                {
                    currentRoundMatches[0].NextMatchId = nextRoundMatches[0].MatchNumber;
                }
                
                // Các matches còn lại của round 1 -> các matches tiếp theo của round 2
                for (int i = 1; i < currentRoundMatches.Count; i++)
                {
                    var nextMatchIndex = i; // Match thứ i của round 1 -> match thứ i của round 2 (bỏ qua match đầu có bye)
                    if (nextMatchIndex < nextRoundMatches.Count)
                    {
                        currentRoundMatches[i].NextMatchId = nextRoundMatches[nextMatchIndex].MatchNumber;
                    }
                }
            }
            else
            {
                // Logic bình thường: 2 matches trong round này tạo ra 1 match trong round sau
                for (int i = 0; i < currentRoundMatches.Count; i++)
                {
                    var nextMatchIndex = i / 2;
                    if (nextMatchIndex < nextRoundMatches.Count)
                    {
                        currentRoundMatches[i].NextMatchId = nextRoundMatches[nextMatchIndex].MatchNumber;
                    }
                }
            }
        }

        return matches;
    }

    private List<AiActivityMatch> GenerateRoundRobinMatches(
        TournamentScheduleRequest request,
        List<ScheduleSlot> slots,
        List<int> classGroupIds,
        ref int matchNumber)
    {
        var matches = new List<AiActivityMatch>();
        var slotIndex = 0;

        // Round robin: mỗi team đấu với tất cả teams khác
        for (int i = 0; i < classGroupIds.Count && slotIndex < slots.Count; i++)
        {
            for (int j = i + 1; j < classGroupIds.Count && slotIndex < slots.Count; j++)
            {
                var slot = slots[slotIndex++];
                var match = new AiActivityMatch
                {
                    ActivityId = request.ActivityId,
                    SportId = request.SportId,
                    ClassGroup1Id = classGroupIds[i],
                    ClassGroup2Id = classGroupIds[j],
                    Grade = request.Grade,
                    MatchDate = slot.MatchDate,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    Location = slot.Location,
                    Status = AiMatchStatus.Pending,
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
    
    // Helper classes cho conflict detection
    private class ParticipantInfo
    {
        public int UserId { get; set; }
        public int ClassGroupId { get; set; }
    }
    
    private class ParticipantActivityConflict
    {
        public int ActivityId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public HashSet<int> ParticipantIds { get; set; } = new();
    }
    
    /// <summary>
    /// Kiểm tra xem hai lớp có lịch học tương đồng không (cùng thứ, overlapping time ranges)
    /// </summary>
    private bool ClassesHaveSimilarTimetable(
        int classAId, 
        int classBId, 
        Dictionary<int, List<ClassGroupSchedule>> schedulesByClassGroup,
        int dayOfWeek)
    {
        if (!schedulesByClassGroup.TryGetValue(classAId, out var schedulesA) ||
            !schedulesByClassGroup.TryGetValue(classBId, out var schedulesB))
        {
            return false;
        }
        
        // Lấy schedules của cả hai lớp cho cùng một thứ
        var schedulesAForDay = schedulesA.Where(s => s.DayOfWeek == dayOfWeek).ToList();
        var schedulesBForDay = schedulesB.Where(s => s.DayOfWeek == dayOfWeek).ToList();
        
        if (!schedulesAForDay.Any() || !schedulesBForDay.Any())
        {
            return false; // Một trong hai lớp không có lịch học thứ này
        }
        
        // Check xem có overlapping time ranges không
        foreach (var scheduleA in schedulesAForDay)
        {
            foreach (var scheduleB in schedulesBForDay)
            {
                // Nếu có overlap → có lịch học tương đồng
                if (scheduleA.StartTime < scheduleB.EndTime && scheduleA.EndTime > scheduleB.StartTime)
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private class ClassGroupScheduleAnalysis
    {
        public HashSet<int> MorningClasses { get; set; } = new(); // Lớp học chủ yếu buổi sáng
        public HashSet<int> AfternoonClasses { get; set; } = new(); // Lớp học chủ yếu buổi chiều
    }
}

