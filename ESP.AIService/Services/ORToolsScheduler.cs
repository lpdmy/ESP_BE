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

        // Tạo dictionary để lookup nhanh lịch học theo ClassGroupId
        var schedulesByClassGroup = classGroupSchedules
            .GroupBy(s => s.ClassGroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        int conflictWithMatches = 0;
        int conflictWithSchedules = 0;
        int conflictWithLocation = 0;

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

        Console.WriteLine($"   ⚠️ Conflicts: {conflictWithMatches} với matches cũ, {conflictWithSchedules} với lịch học, {conflictWithLocation} với location");
        Console.WriteLine($"   ✅ Slots available: {availableSlots.Count}/{slots.Count}");

        return availableSlots;
    }

    private List<ClassGroupSchedule> GetClassGroupSchedules(List<int> classGroupIds, EduShpereDbContext dbContext)
    {
        try
        {
            return dbContext.Set<ClassGroupSchedule>()
                .Where(s => classGroupIds.Contains(s.ClassGroupId) && !s.IsDeleted)
                .ToList();
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
}

