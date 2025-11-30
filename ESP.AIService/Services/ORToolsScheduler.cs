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
        // ========== LOG INPUT TỪ DB ==========
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        Console.WriteLine("📥 INPUT TỪ DB - TournamentScheduleRequest:");
        Console.WriteLine($"   ActivityId: {request.ActivityId}");
        Console.WriteLine($"   SportId: {request.SportId}");
        Console.WriteLine($"   Grade: {request.Grade}");
        Console.WriteLine($"   StartDate: {request.StartDate:yyyy-MM-dd}");
        Console.WriteLine($"   EndDate: {request.EndDate:yyyy-MM-dd}");
        Console.WriteLine($"   MatchDuration: {request.MatchDuration}");
        Console.WriteLine($"   PreferredStartTime: {request.PreferredStartTime}");
        Console.WriteLine($"   PreferredEndTime: {request.PreferredEndTime}");
        Console.WriteLine($"   MaxMatchesPerDay: {request.MaxMatchesPerDay}");
        Console.WriteLine($"   MinGapBetweenMatches: {request.MinGapBetweenMatches} phút");
        Console.WriteLine($"   TournamentFormat: {request.TournamentFormat}");
        Console.WriteLine($"   AvailableLocations: [{string.Join(", ", request.AvailableLocations)}]");
        Console.WriteLine($"   UserNotes: {request.UserNotes}");
        
        // ========== LOG LỚP TỪ DB ==========
        Console.WriteLine($"\n📚 LỚP TỪ DB - ClassGroupIds (Tổng: {request.ClassGroupIds.Count} lớp):");
        for (int i = 0; i < request.ClassGroupIds.Count; i++)
        {
            Console.WriteLine($"   [{i + 1}] ClassGroupId: {request.ClassGroupIds[i]}");
        }
        
        // ========== LOG SLOTS TỪ DB ==========
        Console.WriteLine($"\n⏰ SLOTS TỪ DB (Tổng: {slots.Count} slots):");
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            Console.WriteLine($"   Slot [{i + 1}]:");
            Console.WriteLine($"      - MatchDate: {slot.MatchDate:yyyy-MM-dd}");
            Console.WriteLine($"      - StartTime: {slot.StartTime:hh\\:mm}");
            Console.WriteLine($"      - EndTime: {slot.EndTime:hh\\:mm}");
            Console.WriteLine($"      - Location: {slot.Location ?? "N/A"}");
            Console.WriteLine($"      - MLScore: {slot.MLScore:F2}");
            Console.WriteLine($"      - IsAvailable: {slot.IsAvailable}");
            Console.WriteLine($"      - Explanation: {slot.Explanation}");
        }
        Console.WriteLine("═══════════════════════════════════════════════════════════\n");
        
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
        var filterResult = FilterAvailableSlots(slots, existingMatches, request, dbContext);
        var availableSlots = filterResult.availableSlots;
        var conflictCounts = filterResult.conflictCounts;
        Console.WriteLine($"   Slots available sau khi filter: {availableSlots.Count}");

        if (!availableSlots.Any())
        {
            response.Explanation = "Không có slots nào available sau khi filter conflicts (có thể do trùng với lịch học hoặc matches đã có).";
            return response;
        }

        // Tính số matches cần tạo dựa trên tournament format
        var numberOfMatches = CalculateNumberOfMatches(request.ClassGroupIds.Count, request.TournamentFormat);
        var numberOfRounds = CalculateNumberOfRounds(request.ClassGroupIds.Count, request.TournamentFormat);
        
        // QUAN TRỌNG: Cần chọn THÊM slots để đảm bảo có đủ slots cho tất cả các vòng
        // Ước tính: cần ít nhất numberOfMatches slots (cho vòng 1) + thêm slots cho các vòng sau
        // Với single elimination, số slots tối thiểu = numberOfMatches (vì mỗi match cần 1 slot)
        // Nhưng để đảm bảo có slots cho các vòng sau, chọn thêm ít nhất numberOfRounds slots
        var minSlotsNeeded = numberOfMatches; // Tối thiểu lý thuyết
        var recommendedSlots = numberOfMatches + numberOfRounds; // Khuyến nghị: thêm slots cho các vòng sau
        
        // QUAN TRỌNG: Nếu không đủ slots, điều chỉnh constraint để tránh INFEASIBLE
        // Nếu availableSlots < minSlotsNeeded, giảm minSlotsNeeded xuống availableSlots.Count
        // và cố gắng tạo matches với số slots có sẵn (có thể không đủ matches)
        var actualMinSlots = Math.Min(minSlotsNeeded, availableSlots.Count);
        
        // QUAN TRỌNG: Cho phép chọn TẤT CẢ slots khả dụng (không giới hạn trần)
        // Lý do: Cần có đủ slots trải dài từ ngày đầu đến ngày cuối để xếp cho tất cả các vòng
        // Nếu giới hạn quá ít slots, các vòng sau (Round 3, Chung kết) sẽ không có slot để xếp
        var maxSlotsToSelect = availableSlots.Count; // Cho phép chọn tất cả slots có sẵn
        
        Console.WriteLine($"   📊 Số slots cần chọn: Tối thiểu lý thuyết = {minSlotsNeeded}, Tối thiểu thực tế = {actualMinSlots}, Khuyến nghị = {recommendedSlots}, Tối đa có thể = {availableSlots.Count}");
        Console.WriteLine($"   🔓 Đã mở khóa giới hạn: Cho phép chọn tối đa {maxSlotsToSelect} slots (thay vì giới hạn cũ {recommendedSlots})");
        
        // Cảnh báo nếu không đủ slots
        if (availableSlots.Count < minSlotsNeeded)
        {
            Console.WriteLine($"   ⚠️ CẢNH BÁO: Chỉ có {availableSlots.Count} slots available, nhưng cần tối thiểu {minSlotsNeeded} slots!");
            Console.WriteLine($"      - Sẽ cố gắng tạo matches với số slots có sẵn ({availableSlots.Count} slots)");
            Console.WriteLine($"      - Có thể không tạo đủ {numberOfMatches} matches (chỉ tạo được tối đa {availableSlots.Count} matches)");
        }

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

        // Constraint 1: Chọn ít nhất số slots tối thiểu thực tế, tối đa số slots khuyến nghị
        // QUAN TRỌNG: Sử dụng actualMinSlots thay vì minSlotsNeeded để tránh INFEASIBLE
        var matchConstraint = solver.MakeConstraint(actualMinSlots, maxSlotsToSelect, "slots_range");
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

        // Constraint 3: Không overlap thời gian - đảm bảo các matches không trùng thời gian
        // Với mỗi cặp slots có overlap thời gian, chỉ có thể chọn 1 trong 2
        // TỐI ƯU: Chỉ tạo constraints cho các cặp slots thực sự overlap (giảm số lượng constraints)
        var overlapConstraints = new List<Constraint>();
        var overlapCount = 0;
        for (int i = 0; i < availableSlots.Count; i++)
        {
            for (int j = i + 1; j < availableSlots.Count; j++)
            {
                var slotI = availableSlots[i];
                var slotJ = availableSlots[j];
                
                // Check overlap: cùng ngày và thời gian overlap
                if (slotI.MatchDate.Date == slotJ.MatchDate.Date)
                {
                    // Check time overlap: slotI.StartTime < slotJ.EndTime && slotI.EndTime > slotJ.StartTime
                    if (slotI.StartTime < slotJ.EndTime && slotI.EndTime > slotJ.StartTime)
                    {
                        // Tạo constraint: chỉ có thể chọn 1 trong 2 slots này
                        var overlapConstraint = solver.MakeConstraint(0, 1, $"no_overlap_{i}_{j}");
                        overlapConstraint.SetCoefficient(variables[i], 1);
                        overlapConstraint.SetCoefficient(variables[j], 1);
                        overlapConstraints.Add(overlapConstraint);
                        overlapCount++;
                    }
                }
            }
        }
        
        Console.WriteLine($"   🔒 Đã tạo {overlapCount} constraints để tránh overlap thời gian (tổng {overlapConstraints.Count} constraints)");

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
        var selectedIndices = new HashSet<int>();
        for (int i = 0; i < availableSlots.Count; i++)
        {
            if (variables[i].SolutionValue() > 0.5) // > 0.5 nghĩa là được chọn
            {
                selectedSlots.Add(availableSlots[i]);
                selectedIndices.Add(i);
            }
        }

        // QUAN TRỌNG: Đảm bảo có đủ slots trải đều theo thời gian
        // Với maxSlotsToSelect = availableSlots.Count, OR-Tools sẽ chọn nhiều slots hơn
        // Nhưng vẫn cần kiểm tra và thêm slots nếu cần để đảm bảo có slots cho tất cả các vòng
        // Lưu ý: Không còn giới hạn recommendedSlots nữa, nhưng vẫn cần đảm bảo có đủ slots
        if (selectedSlots.Count < availableSlots.Count)
        {
            Console.WriteLine($"   ⚠️ OR-Tools chỉ chọn {selectedSlots.Count} slots, cần thêm để đảm bảo có đủ slots cho tất cả các vòng");
            
            // Sắp xếp availableSlots theo thời gian và ML score
            var remainingSlots = availableSlots
                .Select((slot, idx) => new { Slot = slot, Index = idx })
                .Where(x => !selectedIndices.Contains(x.Index))
                .OrderByDescending(x => x.Slot.MLScore) // Ưu tiên ML score cao
                .ThenBy(x => x.Slot.MatchDate) // Sau đó ưu tiên thời gian sớm
                .ThenBy(x => x.Slot.StartTime)
                .ToList();
            
            // Thêm slots để đảm bảo có đủ slots cho tất cả các vòng, nhưng đảm bảo không overlap với slots đã chọn
            // Lưu ý: Không còn giới hạn recommendedSlots, nhưng vẫn cần kiểm tra để tránh thêm quá nhiều slots không cần thiết
            var addedCount = 0;
            var targetSlots = Math.Max(recommendedSlots, actualMinSlots * 2); // Đảm bảo có đủ slots cho tất cả các vòng
            foreach (var item in remainingSlots)
            {
                if (selectedSlots.Count >= targetSlots) break;
                
                var slot = item.Slot;
                // Kiểm tra overlap với slots đã chọn
                bool hasOverlap = selectedSlots.Any(s => 
                    s.MatchDate.Date == slot.MatchDate.Date &&
                    s.StartTime < slot.EndTime && s.EndTime > slot.StartTime);
                
                if (!hasOverlap)
                {
                    selectedSlots.Add(slot);
                    selectedIndices.Add(item.Index);
                    addedCount++;
                }
            }
            
            if (addedCount > 0)
            {
                Console.WriteLine($"   ✅ Đã thêm {addedCount} slots để đảm bảo có đủ slots cho tất cả các vòng");
            }
        }

        // Sắp xếp slots theo thời gian
        selectedSlots = selectedSlots.OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).ToList();
        
        Console.WriteLine($"   📊 Tổng số slots được chọn: {selectedSlots.Count} (tối thiểu: {minSlotsNeeded}, khuyến nghị: {recommendedSlots})");

        // Lấy phân tích lịch học để xếp lịch thông minh (chỉ gọi 1 lần để tránh timeout)
        var classGroupSchedules = GetClassGroupSchedules(request.ClassGroupIds, dbContext);
        var classGroupScheduleAnalysis = AnalyzeClassGroupSchedules(classGroupSchedules, request.ClassGroupIds);
        
        // Tạo dictionary schedulesByClassGroup để truyền vào (tránh query lại)
        var schedulesByClassGroup = classGroupSchedules
            .GroupBy(s => s.ClassGroupId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Tạo matches với logic thông minh dựa trên lịch học
        // Lưu biến requiresWeekend để kiểm tra sau
        bool requiresWeekend = false;
        var matches = GenerateMatches(request, selectedSlots, numberOfRounds, ref requiresWeekend, classGroupScheduleAnalysis, dbContext, schedulesByClassGroup);
        
        // QUAN TRỌNG: Sắp xếp matches theo thời gian diễn ra (MatchDate + StartTime)
        // Sau đó đánh lại số thứ tự (MatchNumber) từ 1 đến N theo thứ tự thời gian
        // Điều này đảm bảo trận đấu được đánh số theo thứ tự thời gian thực tế diễn ra
        matches = matches
            .Where(m => m.MatchDate.HasValue && m.StartTime.HasValue) // Chỉ lấy matches đã có thời gian
            .OrderBy(m => m.MatchDate) // Ngày sớm nhất trước
            .ThenBy(m => m.StartTime) // Giờ sớm nhất trước
            .ThenBy(m => m.Round) // Nếu cùng ngày giờ, ưu tiên round thấp hơn
            .ToList();
        
        // Đánh lại số thứ tự (MatchNumber) từ 1 đến N theo thứ tự thời gian
        var matchNumberMap = new Dictionary<int, int>(); // Map từ OldNumber -> NewNumber
        for (int i = 0; i < matches.Count; i++)
        {
            var oldNumber = matches[i].MatchNumber;
            var newNumber = i + 1; // Đánh số từ 1
            matches[i].MatchNumber = newNumber;
            matchNumberMap[oldNumber] = newNumber; // Lưu lại để cập nhật NextMatchId
        }
        
        // Cập nhật NextMatchId theo số mới
        foreach (var match in matches)
        {
            if (match.NextMatchId.HasValue && matchNumberMap.ContainsKey(match.NextMatchId.Value))
            {
                match.NextMatchId = matchNumberMap[match.NextMatchId.Value];
            }
        }
        
        Console.WriteLine($"   ✅ Đã sắp xếp và đánh lại số thứ tự cho {matches.Count} matches theo thời gian diễn ra");

        // KIỂM TRA: Tính số matches dự kiến (Single Elimination: n-1 matches)
        // QUAN TRỌNG: Khi chia nhánh, số matches thực tế có thể khác do logic chia nhánh
        // Với single elimination, số matches tối thiểu = n - 1 (không tính chung kết giữa nhánh)
        // Nhưng nếu chia nhánh, có thể có thêm 1 match chung kết
        var expectedMatches = request.ClassGroupIds.Count - 1;
        
        // QUAN TRỌNG: Nếu không đủ slots, điều chỉnh expectedMatches
        // Nếu chỉ có actualMinSlots slots, chỉ có thể tạo tối đa actualMinSlots matches
        var maxPossibleMatches = Math.Min(expectedMatches, actualMinSlots);
        var isComplete = matches.Count >= maxPossibleMatches;
        
        // Nếu không đủ slots, vẫn coi là success nếu tạo được tất cả matches có thể
        var isSuccess = isComplete || (availableSlots.Count < minSlotsNeeded && matches.Count == availableSlots.Count);
        
        response.Success = isSuccess; // Success nếu tạo đủ matches hoặc tạo hết slots có thể
        response.IsOptimal = resultStatus == Solver.ResultStatus.OPTIMAL && isComplete;
        response.GeneratedMatches = matches;
        response.ObjectiveValue = (float)solver.Objective().Value();
        response.TotalMatches = matches.Count;
        response.TotalRounds = numberOfRounds;
        
        if (!isComplete)
        {
            if (availableSlots.Count < minSlotsNeeded)
            {
                response.Explanation = $"⚠️ CẢNH BÁO: Chỉ có {availableSlots.Count} slots available (cần {minSlotsNeeded} slots). " +
                                      $"Đã tạo {matches.Count}/{maxPossibleMatches} matches có thể. " +
                                      $"Thiếu {expectedMatches - matches.Count} matches so với yêu cầu. " +
                                      $"Lý do: Quá nhiều conflicts với activities khác của participants ({conflictCounts.conflictWithParticipantActivities} conflicts). " +
                                      $"Đề xuất: Mở rộng khoảng thời gian hoặc giảm conflicts. " +
                                      $"Objective value: {response.ObjectiveValue:F2}";
            }
            else
            {
                response.Explanation = $"⚠️ CẢNH BÁO: Chỉ tạo được {matches.Count}/{expectedMatches} matches! " +
                                      $"Thiếu {expectedMatches - matches.Count} matches. " +
                                      $"Lý do: Không đủ slots hợp lệ để tạo đủ các vòng. " +
                                      $"Objective value: {response.ObjectiveValue:F2}";
            }
        }
        else
        {
            response.Explanation = $"Đã tạo {matches.Count} matches trong {numberOfRounds} rounds. " +
                                  $"Objective value: {response.ObjectiveValue:F2}";
        }
        
        // QUAN TRỌNG: Nếu yêu cầu thêm ngày cuối tuần, thêm vào explanation
        if (requiresWeekend)
        {
            var weekendMessage = "⚠️ YÊU CẦU: Cần thêm ngày thứ 7, chủ nhật để đảm bảo sức khỏe học sinh cho trận chung kết giữa 2 nhóm. " +
                                "Lý do: Nhóm A (học sáng) chỉ đá được chiều (13:00-18:00), Nhóm B (học chiều) chỉ đá được sáng (07:00-11:30). " +
                                "Giải pháp: Thêm ngày thứ 7, chủ nhật để cả 2 nhóm đều rảnh và có thể thi đấu trận chung kết.";
            
            if (string.IsNullOrEmpty(response.Explanation))
            {
                response.Explanation = weekendMessage;
            }
            else if (!response.Explanation.Contains("thứ 7, chủ nhật"))
            {
                response.Explanation = weekendMessage + " " + response.Explanation;
            }
        }

        return response;
    }

    private (List<ScheduleSlot> availableSlots, (int conflictWithMatches, int conflictWithLocation, int conflictWithParticipantActivities) conflictCounts) FilterAvailableSlots(
        List<ScheduleSlot> slots,
        List<AiActivityMatch> existingMatches,
        TournamentScheduleRequest request,
        EduShpereDbContext dbContext)
    {
        var availableSlots = new List<ScheduleSlot>();

        // Lấy lịch học của các lớp tham gia
        Console.WriteLine($"\n🔍 Đang lấy lịch học từ DB cho {request.ClassGroupIds.Count} lớp...");
        var classGroupSchedules = GetClassGroupSchedules(request.ClassGroupIds, dbContext);
        Console.WriteLine($"   ✅ Đã load {classGroupSchedules.Count} lịch học từ {request.ClassGroupIds.Count} lớp tham gia");

        // Lấy tất cả participants của các lớp tham gia để check conflict với activities khác
        Console.WriteLine($"\n🔍 Đang lấy participants từ DB cho {request.ClassGroupIds.Count} lớp...");
        var participantsInClassGroups = GetParticipantsInClassGroups(request.ClassGroupIds, dbContext);
        Console.WriteLine($"   ✅ Đã load {participantsInClassGroups.Count} participants từ {request.ClassGroupIds.Count} lớp tham gia");
        
        // ========== LOG PARTICIPANTS TỪ DB ==========
        if (participantsInClassGroups.Any())
        {
            Console.WriteLine($"\n👥 PARTICIPANTS TỪ DB (Tổng: {participantsInClassGroups.Count} participants):");
            var participantsByClassGroup = participantsInClassGroups.GroupBy(p => p.ClassGroupId).ToList();
            foreach (var group in participantsByClassGroup)
            {
                Console.WriteLine($"   ClassGroupId: {group.Key} ({group.Count()} participants):");
                var userIds = group.Select(p => p.UserId).ToList();
                Console.WriteLine($"      - UserIds: [{string.Join(", ", userIds)}]");
            }
        }
        
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

            // KHÔNG check conflict với lịch học ở đây
            // Lý do: Khi filter slots, chúng ta chưa biết match nào sẽ được gán cho slot nào
            // Sẽ check conflict lịch học khi tạo match cụ thể (chỉ check 2 lớp tham gia match đó)
            // Điều này cho phép linh hoạt hơn: nếu lịch thi eo hẹp, vẫn có thể tạo match
            // miễn là 2 lớp tham gia match đó rảnh
            
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
                
                // Heuristic 2: Ưu tiên slot mà nhiều lớp đều trống cùng lúc (không bắt buộc tất cả)
                // Chỉ tính điểm bonus, không loại bỏ slot
                var dayOfWeek = (int)slot.MatchDate.DayOfWeek;
                var dayOfWeekNormalized = dayOfWeek == 0 ? 7 : dayOfWeek;
                int classesWithFreeSlot = 0;
                
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
                
                // Tăng điểm dựa trên số lớp rảnh (không bắt buộc tất cả)
                var freeSlotRatio = request.ClassGroupIds.Count > 0 
                    ? (float)classesWithFreeSlot / request.ClassGroupIds.Count 
                    : 0f;
                
                if (freeSlotRatio >= 0.8f) // 80%+ lớp rảnh
                {
                    heuristicBonus += 10.0f;
                }
                else if (freeSlotRatio >= 0.5f) // 50-80% lớp rảnh
                {
                    heuristicBonus += 5.0f;
                }
                else if (freeSlotRatio > 0f) // Có ít nhất 1 lớp rảnh
                {
                    heuristicBonus += 2.0f;
                }
                
                // Heuristic 3: Ưu tiên đặc biệt cho slot 11h-12h (giữa 2 buổi học)
                // Đây là khoảng thời gian lý tưởng vì các lớp học sáng (7h-11h) và chiều (12h-17h) đều rảnh
                var slotStartHour = slot.StartTime.Hours;
                var slotStartMinute = slot.StartTime.Minutes;
                var slotStartTotalMinutes = slotStartHour * 60 + slotStartMinute;
                var slotEndTotalMinutes = slot.EndTime.Hours * 60 + slot.EndTime.Minutes;
                
                // Khoảng 11h-12h là lý tưởng (660-720 phút)
                if (slotStartTotalMinutes >= 660 && slotEndTotalMinutes <= 720)
                {
                    heuristicBonus += 15.0f; // Bonus cao cho slot giữa 2 buổi học
                }
                // Khoảng 11h-12h30 cũng tốt
                else if (slotStartTotalMinutes >= 660 && slotEndTotalMinutes <= 750)
                {
                    heuristicBonus += 10.0f;
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

            // QUAN TRỌNG: Loại bỏ slots trong khung giờ CẤM (Nghỉ trưa): 11:30 - 13:00
            // Không có trận đấu nào được diễn ra hoặc vắt qua khung giờ này
            if (isAvailable)
            {
                var slotStartTotalMinutes = slot.StartTime.Hours * 60 + slot.StartTime.Minutes;
                var slotEndTotalMinutes = slot.EndTime.Hours * 60 + slot.EndTime.Minutes;
                var forbiddenStartMinutes = 11 * 60 + 30; // 11:30
                var forbiddenEndMinutes = 13 * 60; // 13:00
                
                // Check nếu slot overlap với khung giờ cấm
                if (slotStartTotalMinutes < forbiddenEndMinutes && slotEndTotalMinutes > forbiddenStartMinutes)
                {
                    isAvailable = false;
                    conflictWithSchedules++;
                    continue; // Bỏ qua slot này
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

        Console.WriteLine($"   ⚠️ Conflicts: {conflictWithMatches} với matches cũ, {conflictWithLocation} với location, {conflictWithParticipantActivities} với activities khác của participants");
        Console.WriteLine($"   ℹ️ Lưu ý: Không filter slots dựa trên lịch học của tất cả lớp. Sẽ check conflict lịch học khi tạo match cụ thể.");
        Console.WriteLine($"   ✅ Slots available: {availableSlots.Count}/{slots.Count}");

        return (availableSlots, (conflictWithMatches, conflictWithLocation, conflictWithParticipantActivities));
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
            
            // ========== LOG SCHEDULES TỪ DB (ClassGroupSchedule) ==========
            Console.WriteLine($"\n📅 SCHEDULES TỪ DB - ClassGroupSchedule (Tổng: {schedules.Count} schedules):");
            var schedulesByClassGroup = schedules.GroupBy(s => s.ClassGroupId).ToList();
            foreach (var group in schedulesByClassGroup)
            {
                Console.WriteLine($"   ClassGroupId: {group.Key} ({group.Count()} schedules):");
                foreach (var schedule in group)
                {
                    Console.WriteLine($"      - DayOfWeek: {schedule.DayOfWeek} ({(DayOfWeek)(schedule.DayOfWeek == 7 ? 0 : schedule.DayOfWeek)})");
                    Console.WriteLine($"      - StartTime: {schedule.StartTime:hh\\:mm}");
                    Console.WriteLine($"      - EndTime: {schedule.EndTime:hh\\:mm}");
                    Console.WriteLine($"      - Subject: {schedule.Subject ?? "N/A"}");
                    Console.WriteLine($"      - Period: {schedule.Period}");
                }
            }
            
            // Lấy lịch học từ Timetable (lịch học import từ CSV/Excel/ICS)
            try
            {
                var timetables = dbContext.Set<Timetable>()
                    .Where(t => classGroupIds.Contains(t.ClassGroupId) && !t.IsDeleted)
                    .ToList();
                
                // ========== LOG SCHEDULES TỪ DB (Timetable) ==========
                Console.WriteLine($"\n📅 SCHEDULES TỪ DB - Timetable (Tổng: {timetables.Count} timetables):");
                var timetablesByClassGroup = timetables.GroupBy(t => t.ClassGroupId).ToList();
                foreach (var group in timetablesByClassGroup)
                {
                    Console.WriteLine($"   ClassGroupId: {group.Key} ({group.Count()} timetables):");
                    foreach (var timetable in group)
                    {
                        Console.WriteLine($"      - DayOfWeek: {timetable.DayOfWeek} ({(DayOfWeek)(timetable.DayOfWeek == 7 ? 0 : timetable.DayOfWeek)})");
                        Console.WriteLine($"      - StartTime: {timetable.StartTime:hh\\:mm}");
                        Console.WriteLine($"      - EndTime: {timetable.EndTime:hh\\:mm}");
                        Console.WriteLine($"      - SubjectName: {timetable.SubjectName ?? "N/A"}");
                    }
                }
                
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
                
                Console.WriteLine($"\n📊 TỔNG HỢP: {schedules.Count} schedules (ClassGroupSchedule: {schedules.Count - timetableSchedules.Count}, Timetable: {timetableSchedules.Count})");
            }
            catch (Exception ex)
            {
                // Nếu bảng Timetable chưa tồn tại, chỉ dùng ClassGroupSchedule
                Console.WriteLine($"   ⚠️ Bảng Timetables chưa tồn tại hoặc có lỗi: {ex.Message}");
                Console.WriteLine("   Chỉ sử dụng ClassGroupSchedule");
            }
            
            return schedules;
        }
        catch (Exception ex)
        {
            // Nếu table chưa tồn tại hoặc có lỗi
            Console.WriteLine($"   ❌ Lỗi khi lấy ClassGroupSchedules từ DB: {ex.Message}");
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
        int numberOfRounds,
        ref bool requiresWeekend,
        ClassGroupScheduleAnalysis? scheduleAnalysis = null,
        EduShpereDbContext? dbContext = null,
        Dictionary<int, List<ClassGroupSchedule>>? schedulesByClassGroup = null)
    {
        var matches = new List<AiActivityMatch>();
        var classGroupIds = request.ClassGroupIds.ToList();
        var matchNumber = 1;

        // Single Elimination Tournament
        if (request.TournamentFormat.ToLower() == "singleelimination")
        {
            matches = GenerateSingleEliminationMatches(request, slots, classGroupIds, numberOfRounds, ref matchNumber, ref requiresWeekend, scheduleAnalysis, dbContext, schedulesByClassGroup);
        }
        // Round Robin Tournament
        else if (request.TournamentFormat.ToLower() == "roundrobin")
        {
            matches = GenerateRoundRobinMatches(request, slots, classGroupIds, ref matchNumber, scheduleAnalysis);
        }
        // Default: Single Elimination
        else
        {
            matches = GenerateSingleEliminationMatches(request, slots, classGroupIds, numberOfRounds, ref matchNumber, ref requiresWeekend, scheduleAnalysis, dbContext, schedulesByClassGroup);
        }

        return matches;
    }

    private List<AiActivityMatch> GenerateSingleEliminationMatches(
        TournamentScheduleRequest request,
        List<ScheduleSlot> slots,
        List<int> classGroupIds,
        int numberOfRounds,
        ref int matchNumber,
        ref bool requiresWeekend,
        ClassGroupScheduleAnalysis? scheduleAnalysis = null,
        EduShpereDbContext? dbContext = null,
        Dictionary<int, List<ClassGroupSchedule>>? schedulesByClassGroup = null)
    {
        var matches = new List<AiActivityMatch>();

        // Phân loại lớp theo buổi học để xếp lịch thông minh
        var morningClasses = scheduleAnalysis?.MorningClasses ?? new HashSet<int>();
        var afternoonClasses = scheduleAnalysis?.AfternoonClasses ?? new HashSet<int>();
        
        // PHÂN LOẠI: Tách các đội thành 2 nhóm theo quy tắc mới:
        // Nhóm A (Học sáng - Chỉ đá được chiều): Rảnh từ 13:00 - 18:00
        // Nhóm B (Học chiều - Chỉ đá được sáng): Rảnh từ 07:00 - 11:00
        // QUAN TRỌNG: Logic đảo ngược so với trước:
        // - MorningClasses = các lớp học buổi sáng → thuộc Nhóm A (chỉ đá được chiều)
        // - AfternoonClasses = các lớp học buổi chiều → thuộc Nhóm B (chỉ đá được sáng)
        var groupA_AfternoonOnly = classGroupIds.Where(id => morningClasses.Contains(id)).ToList(); // Nhóm A: Học sáng, đá chiều
        var groupB_MorningOnly = classGroupIds.Where(id => afternoonClasses.Contains(id)).ToList(); // Nhóm B: Học chiều, đá sáng
        var unknownTeams = classGroupIds.Where(id => !morningClasses.Contains(id) && !afternoonClasses.Contains(id)).ToList();
        
        // Phân bổ các lớp chưa xác định: ưu tiên thêm vào nhóm ít hơn để cân bằng
        if (unknownTeams.Count > 0)
        {
            foreach (var team in unknownTeams)
            {
                if (groupA_AfternoonOnly.Count <= groupB_MorningOnly.Count)
                {
                    groupA_AfternoonOnly.Add(team);
                }
                else
                {
                    groupB_MorningOnly.Add(team);
                }
            }
        }
        
        Console.WriteLine($"   📊 Phân loại nhóm: {groupA_AfternoonOnly.Count} đội Nhóm A (Học sáng, đá chiều 13:00-18:00), {groupB_MorningOnly.Count} đội Nhóm B (Học chiều, đá sáng 07:00-11:00)");
        
        // Phân loại slots theo quy tắc mới:
        // - Nhóm B (đá sáng): 07:00 - 11:30 (End Time không được vượt quá 11:30)
        // - Nhóm A (đá chiều): 13:00 - 18:00 (Start Time phải từ 13:00)
        // QUAN TRỌNG: KHÔNG có crossover slots. Nếu cần trận chung kết giữa 2 nhóm, yêu cầu thêm ngày thứ 7, chủ nhật
        var morningSlots = slots.Where(s => {
            var startTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
            var endTotalMinutes = s.EndTime.Hours * 60 + s.EndTime.Minutes;
            // Buổi sáng: 07:00 - 11:30 (End Time không được vượt quá 11:30)
            return startTotalMinutes >= 7 * 60 && endTotalMinutes <= 11 * 60 + 30;
        }).OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).ToList();
        
        var afternoonSlots = slots.Where(s => {
            var startTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
            // Buổi chiều: 13:00 - 18:00 (Start Time phải từ 13:00)
            return startTotalMinutes >= 13 * 60 && s.StartTime.Hours < 18;
        }).OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).ToList();
        
        var otherSlots = slots.Where(s => {
            var startTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
            var endTotalMinutes = s.EndTime.Hours * 60 + s.EndTime.Minutes;
            // Loại bỏ slots trong khung giờ cấm và các slots không phù hợp
            var isMorning = startTotalMinutes >= 7 * 60 && endTotalMinutes <= 11 * 60 + 30;
            var isAfternoon = startTotalMinutes >= 13 * 60 && s.StartTime.Hours < 18;
            return !isMorning && !isAfternoon;
        }).OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).ToList();
        
        Console.WriteLine($"   ⏰ Phân loại slots: {morningSlots.Count} buổi sáng (07:00-11:30), {afternoonSlots.Count} buổi chiều (13:00-18:00), {otherSlots.Count} khác");
        
        // Tạo danh sách slots đã sử dụng để track
        var usedSlots = new HashSet<ScheduleSlot>();

        // CÂN BẰNG NHÁNH: Tạo 2 nhánh đấu riêng biệt theo quy tắc mới
        // Nhánh 1: Nhóm B (Học chiều, đá sáng) → đấu vào buổi Sáng (07:00-11:30)
        // Nhánh 2: Nhóm A (Học sáng, đá chiều) → đấu vào buổi Chiều (13:00-18:00)
        // QUAN TRỌNG: KHÔNG có crossover slots. Nếu cần trận chung kết giữa 2 nhóm, yêu cầu thêm ngày thứ 7, chủ nhật
        
        // Sử dụng schedulesByClassGroup đã được truyền vào (tránh query lại database)
        if (schedulesByClassGroup == null)
        {
            schedulesByClassGroup = new Dictionary<int, List<ClassGroupSchedule>>();
            if (dbContext != null)
            {
                var classGroupSchedules = GetClassGroupSchedules(request.ClassGroupIds, dbContext);
                schedulesByClassGroup = classGroupSchedules
                    .GroupBy(s => s.ClassGroupId)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
        }
        
        // Xử lý Bye cho từng nhóm riêng biệt
        // Đặt tên nhánh theo số đội tham gia (chuyên nghiệp hơn)
        var groupB_MorningBranchMatches = GenerateBranchMatches(
            request, groupB_MorningOnly, morningSlots, otherSlots, 
            $"Nhánh {groupB_MorningOnly.Count} đội (Nhóm B - Đá sáng)", "Nhóm B", ref matchNumber, ref usedSlots, schedulesByClassGroup, 
            minStartTime: TimeSpan.FromHours(7), maxEndTime: TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30)));
        
        var groupA_AfternoonBranchMatches = GenerateBranchMatches(
            request, groupA_AfternoonOnly, afternoonSlots, otherSlots, 
            $"Nhánh {groupA_AfternoonOnly.Count} đội (Nhóm A - Đá chiều)", "Nhóm A", ref matchNumber, ref usedSlots, schedulesByClassGroup,
            minStartTime: TimeSpan.FromHours(13), maxEndTime: TimeSpan.FromHours(18));
        
        // Gộp matches từ 2 nhánh
        matches.AddRange(groupB_MorningBranchMatches);
        matches.AddRange(groupA_AfternoonBranchMatches);
        
        // Xử lý trận Chung kết giữa 2 nhánh (Nhóm A vs Nhóm B)
        // QUAN TRỌNG: KHÔNG có crossover slots. Nếu không có slot phù hợp, yêu cầu thêm ngày thứ 7, chủ nhật
        var (finalMatches, finalRequiresWeekend) = GenerateFinalMatchesBetweenBranches(
            request, groupB_MorningBranchMatches, groupA_AfternoonBranchMatches, 
            slots, ref matchNumber, ref usedSlots);
        
        matches.AddRange(finalMatches);
        
        // Nếu yêu cầu thêm ngày cuối tuần, cập nhật biến requiresWeekend
        if (finalRequiresWeekend)
        {
            requiresWeekend = true;
            Console.WriteLine($"   ⚠️ YÊU CẦU: Cần thêm ngày thứ 7, chủ nhật để đảm bảo sức khỏe học sinh cho trận chung kết giữa 2 nhóm!");
        }
        
        Console.WriteLine($"   ✅ Đã tạo {matches.Count} matches: {groupB_MorningBranchMatches.Count} nhánh Nhóm B (Sáng), {groupA_AfternoonBranchMatches.Count} nhánh Nhóm A (Chiều), {finalMatches.Count} trận chung kết");
        
        // KIỂM TRA: Đảm bảo đã tạo đủ matches cho tất cả các vòng
        var expectedGroupBMatches = CalculateExpectedMatchesForBranch(groupB_MorningOnly.Count);
        var expectedGroupAMatches = CalculateExpectedMatchesForBranch(groupA_AfternoonOnly.Count);
        var expectedTotalMatches = expectedGroupBMatches + expectedGroupAMatches + (finalMatches.Count > 0 ? 1 : 0);
        
        if (matches.Count < expectedTotalMatches)
        {
            Console.WriteLine($"   ⚠️ CẢNH BÁO: Chỉ tạo được {matches.Count}/{expectedTotalMatches} matches!");
            Console.WriteLine($"      - Nhánh Nhóm B (Sáng): {groupB_MorningBranchMatches.Count}/{expectedGroupBMatches} matches");
            Console.WriteLine($"      - Nhánh Nhóm A (Chiều): {groupA_AfternoonBranchMatches.Count}/{expectedGroupAMatches} matches");
            Console.WriteLine($"      - Chung kết: {finalMatches.Count}/1 matches");
            Console.WriteLine($"   ⚠️ Lý do: Không đủ slots hợp lệ để tạo đủ các vòng!");
        }
        
        return matches;
    }
    
    /// <summary>
    /// Helper: Tính số matches dự kiến cho một nhánh
    /// </summary>
    private int CalculateExpectedMatchesForBranch(int teamCount)
    {
        if (teamCount <= 0) return 0;
        if (teamCount == 1) return 0; // Chỉ có 1 đội, không cần đá
        
        var powerOfTwo = GetNextPowerOfTwo(teamCount);
        var byeCount = powerOfTwo - teamCount;
        var playCount = teamCount - byeCount;
        
        // Số matches = số teams - 1 (single elimination)
        return powerOfTwo - 1;
    }
    
    /// <summary>
    /// Helper: Tính lũy thừa của 2 gần nhất >= N
    /// </summary>
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
    
    /// <summary>
    /// Helper: Tính số bye và số đội phải đá vòng 1
    /// </summary>
    private (int ByeCount, int PlayCount, int PowerOfTwo) CalculateByeInfo(int teamCount)
    {
        var powerOfTwo = GetNextPowerOfTwo(teamCount);
        var byeCount = powerOfTwo - teamCount;
        var playCount = teamCount - byeCount;
        return (byeCount, playCount, powerOfTwo);
    }
    
    /// <summary>
    /// Check xem slot có conflict với lịch học của 2 lớp cụ thể không
    /// </summary>
    private bool HasScheduleConflict(
        ScheduleSlot slot,
        int? classGroup1Id,
        int? classGroup2Id,
        Dictionary<int, List<ClassGroupSchedule>> schedulesByClassGroup)
    {
        if (!classGroup1Id.HasValue && !classGroup2Id.HasValue)
            return false; // Không có lớp nào → không conflict
        
        var dayOfWeek = (int)slot.MatchDate.DayOfWeek;
        var dayOfWeekNormalized = dayOfWeek == 0 ? 7 : dayOfWeek;
        
        // Check conflict với lớp 1
        if (classGroup1Id.HasValue && schedulesByClassGroup.TryGetValue(classGroup1Id.Value, out var schedules1))
        {
            foreach (var schedule in schedules1)
            {
                if (schedule.DayOfWeek == dayOfWeekNormalized)
                {
                    if (slot.StartTime < schedule.EndTime && slot.EndTime > schedule.StartTime)
                    {
                        return true; // Conflict với lớp 1
                    }
                }
            }
        }
        
        // Check conflict với lớp 2
        if (classGroup2Id.HasValue && schedulesByClassGroup.TryGetValue(classGroup2Id.Value, out var schedules2))
        {
            foreach (var schedule in schedules2)
            {
                if (schedule.DayOfWeek == dayOfWeekNormalized)
                {
                    if (slot.StartTime < schedule.EndTime && slot.EndTime > schedule.StartTime)
                    {
                        return true; // Conflict với lớp 2
                    }
                }
            }
        }
        
        return false; // Không conflict
    }
    
    /// <summary>
    /// Tạo matches cho một nhánh đấu
    /// </summary>
    private List<AiActivityMatch> GenerateBranchMatches(
        TournamentScheduleRequest request,
        List<int> teams,
        List<ScheduleSlot> preferredSlots,
        List<ScheduleSlot> fallbackSlots,
        string branchName,
        string teamType,
        ref int matchNumber,
        ref HashSet<ScheduleSlot> usedSlots,
        Dictionary<int, List<ClassGroupSchedule>> schedulesByClassGroup,
        TimeSpan? minStartTime = null,
        TimeSpan? maxEndTime = null)
    {
        var matches = new List<AiActivityMatch>();
        
        if (teams.Count == 0)
        {
            Console.WriteLine($"   ⚠️ {branchName}: Không có đội nào");
            return matches;
        }
        
        // Tính bye cho nhóm này
        var (byeCount, playCount, powerOfTwo) = CalculateByeInfo(teams.Count);
        Console.WriteLine($"   📋 {branchName}: {teams.Count} đội → Power of 2: {powerOfTwo}, Bye: {byeCount}, Đá vòng 1: {playCount}");
        
        // Shuffle teams để random
        var random = new Random();
        var shuffledTeams = teams.OrderBy(x => random.Next()).ToList();
        
        // Chọn đội được bye (random)
        var byeTeams = new List<int>();
        for (int i = 0; i < byeCount && shuffledTeams.Count > 0; i++)
        {
            var byeIndex = random.Next(shuffledTeams.Count);
            byeTeams.Add(shuffledTeams[byeIndex]);
            shuffledTeams.RemoveAt(byeIndex);
        }

        // Tính số rounds cho nhánh này
        var numberOfRounds = (int)Math.Ceiling(Math.Log2(powerOfTwo));
        Console.WriteLine($"   📊 {branchName}: Số rounds cần tạo = {numberOfRounds} (power of 2 = {powerOfTwo})");
        
        // Copy usedSlots để dùng trong lambda (tránh lỗi ref trong lambda)
        var usedSlotsCopy = usedSlots;
        
        // Tạo matches cho vòng 1 (các đội phải đá)
        // QUAN TRỌNG: Vòng 1 phải lấp đầy các slot sớm nhất có thể (từ ngày 01/12)
        // Không được để trống slot ngày Thứ 2, Thứ 3 nếu vẫn còn trận Vòng 1 chưa xếp
        // QUAN TRỌNG: Ưu tiên slots ngày thường (Thứ 2 - Thứ 6), nhưng nếu thiếu thì dùng slots cuối tuần (Fallback Logic)
        var round1Matches = new List<AiActivityMatch>();
        var allAvailableSlots = preferredSlots.Where(s => !usedSlotsCopy.Contains(s)).ToList();
        allAvailableSlots.AddRange(fallbackSlots.Where(s => !usedSlotsCopy.Contains(s)));
        
        // Bước 1: Lấy slots ngày thường (Thứ 2 - Thứ 6)
        var weekdaySlots = allAvailableSlots
            .Where(s => {
                var dayOfWeek = s.MatchDate.DayOfWeek;
                return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
            })
            .OrderBy(s => s.MatchDate)
            .ThenBy(s => s.StartTime)
            .ThenBy(s => preferredSlots.Contains(s) ? 0 : 1) // Preferred slots trước
            .ToList();
        
        // Bước 2: Lấy slots cuối tuần (Thứ 7, Chủ Nhật)
        var weekendSlots = allAvailableSlots
            .Where(s => {
                var dayOfWeek = s.MatchDate.DayOfWeek;
                return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
            })
            .OrderBy(s => s.MatchDate)
            .ThenBy(s => s.StartTime)
            .ThenBy(s => preferredSlots.Contains(s) ? 0 : 1) // Preferred slots trước
            .ToList();
        
        Console.WriteLine($"   📅 Round 1: Slots ngày thường: {weekdaySlots.Count}, Slots cuối tuần: {weekendSlots.Count} (tổng: {allAvailableSlots.Count})");
        
        // Bước 3: CHECK THÔNG MINH (Fallback Logic)
        // Tính toán số slots cần thiết cho vòng này (số trận cần đá)
        int matchesNeeded = playCount / 2; // Ví dụ: 4 đội đá thì cần 2 trận -> 2 slots
        List<ScheduleSlot> sortedByTime;
        
        if (weekdaySlots.Count >= matchesNeeded)
        {
            // Nếu ngày thường đủ slot -> Dùng ngày thường (Ưu tiên số 1)
            Console.WriteLine($"   ✅ Round 1: Đủ slots ngày thường ({weekdaySlots.Count} slots >= {matchesNeeded} cần). Bỏ qua cuối tuần.");
            sortedByTime = weekdaySlots;
        }
        else
        {
            // CỨU CÁNH: Nếu ngày thường thiếu -> GỘP thêm slots cuối tuần vào
            Console.WriteLine($"   ⚠️ Round 1: Thiếu slots ngày thường (chỉ có {weekdaySlots.Count}, cần {matchesNeeded}). KÍCH HOẠT FALLBACK: Sử dụng cả cuối tuần.");
            
            sortedByTime = new List<ScheduleSlot>();
            sortedByTime.AddRange(weekdaySlots);
            sortedByTime.AddRange(weekendSlots);
            
            // Sắp xếp lại toàn bộ theo thời gian
            sortedByTime = sortedByTime
                .OrderBy(s => s.MatchDate)
                .ThenBy(s => s.StartTime)
                .ThenBy(s => preferredSlots.Contains(s) ? 0 : 1)
                .ToList();
        }
        
        if (sortedByTime.Any())
        {
            Console.WriteLine($"   📅 Round 1: Slots được sắp xếp theo thứ tự thời gian (từ {sortedByTime.First().MatchDate:yyyy-MM-dd} đến {sortedByTime.Last().MatchDate:yyyy-MM-dd})");
            Console.WriteLine($"   📅 Round 1: Slot đầu tiên = {sortedByTime.First().MatchDate:yyyy-MM-dd} {sortedByTime.First().StartTime:hh\\:mm} ({sortedByTime.First().MatchDate.DayOfWeek})");
            Console.WriteLine($"   📅 Round 1: Slot cuối cùng = {sortedByTime.Last().MatchDate:yyyy-MM-dd} {sortedByTime.Last().StartTime:hh\\:mm} ({sortedByTime.Last().MatchDate.DayOfWeek})");
        }
        else
        {
            Console.WriteLine($"   ❌ Round 1: KHÔNG CÓ slots nào để sử dụng!");
        }
        
        int matchCount = 0;
        int slotIndex = 0;
        // QUAN TRỌNG: Đảm bảo có đủ teams để tạo matches (playCount phải chẵn)
        int maxMatches = playCount / 2;
        for (int i = 0; i < maxMatches && matchCount < maxMatches && (i * 2 + 1) < shuffledTeams.Count; i++)
        {
            var team1 = shuffledTeams[i * 2];
            var team2 = shuffledTeams[i * 2 + 1];
            
            // Tìm slot phù hợp cho 2 lớp này
            // Ưu tiên: slot không conflict với lịch học của 2 lớp
            // Nếu không có, vẫn chọn slot (không bắt buộc tất cả lớp phải rảnh)
            ScheduleSlot? selectedSlot = null;
            bool foundNonConflictSlot = false;
            
            // Bước 1: Tìm slot không conflict với lịch học của 2 lớp (ưu tiên)
            // QUAN TRỌNG: Phải lấp đầy slots theo thứ tự thời gian, không được bỏ qua
            // QUAN TRỌNG: Không được để trống slot ngày Thứ 2, Thứ 3 nếu vẫn còn trận Round 1 chưa xếp
            for (int j = slotIndex; j < sortedByTime.Count; j++)
            {
                var slot = sortedByTime[j];
                if (usedSlotsCopy.Contains(slot)) continue;
                
                // QUAN TRỌNG: Kiểm tra ràng buộc thời gian (minStartTime, maxEndTime)
                if (minStartTime.HasValue)
                {
                    var slotStartTotalMinutes = slot.StartTime.Hours * 60 + slot.StartTime.Minutes;
                    var minStartTotalMinutes = minStartTime.Value.Hours * 60 + minStartTime.Value.Minutes;
                    if (slotStartTotalMinutes < minStartTotalMinutes) continue;
                }
                if (maxEndTime.HasValue)
                {
                    var slotEndTotalMinutes = slot.EndTime.Hours * 60 + slot.EndTime.Minutes;
                    var maxEndTotalMinutes = maxEndTime.Value.Hours * 60 + maxEndTime.Value.Minutes;
                    if (slotEndTotalMinutes > maxEndTotalMinutes) continue;
                }
                
                // QUAN TRỌNG: Đảm bảo thời gian nghỉ 30 phút giữa các trận
                // Start Time của trận sau >= End Time của trận trước + 30 phút nghỉ
                bool hasEnoughGap = true;
                foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                {
                    if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                    {
                        var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                        var slotStart = slot.MatchDate.Date.Add(slot.StartTime);
                        var gapMinutes = (slotStart - usedMatchEnd).TotalMinutes;
                        if (gapMinutes < 30) // Cần ít nhất 30 phút nghỉ
                        {
                            hasEnoughGap = false;
                            break;
                        }
                    }
                }
                if (!hasEnoughGap) continue;
                
                // Check conflict với lịch học của 2 lớp tham gia match này
                if (!HasScheduleConflict(slot, team1, team2, schedulesByClassGroup))
                {
                    selectedSlot = slot;
                    foundNonConflictSlot = true;
                    slotIndex = j + 1;
                    Console.WriteLine($"   ✅ Round 1 Match {matchCount + 1}: Chọn slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} ({slot.MatchDate.DayOfWeek}) - Không conflict");
                    break;
                }
            }
            
            // Bước 2: Nếu không tìm thấy slot rảnh, chọn slot đầu tiên còn lại theo thứ tự thời gian
            // QUAN TRỌNG: Vẫn phải tuân theo ràng buộc thời gian và thời gian nghỉ
            // QUAN TRỌNG: KHÔNG được bỏ qua slots ngày Thứ 2, Thứ 3 nếu vẫn còn trận Round 1 chưa xếp
            // Phải lấp đầy các slot sớm nhất có thể, không được nhảy cóc
            if (selectedSlot == null)
            {
                for (int j = slotIndex; j < sortedByTime.Count; j++)
                {
                    var slot = sortedByTime[j];
                    if (usedSlotsCopy.Contains(slot)) continue;
                    
                    // Kiểm tra ràng buộc thời gian
                    if (minStartTime.HasValue)
                    {
                        var slotStartTotalMinutes = slot.StartTime.Hours * 60 + slot.StartTime.Minutes;
                        var minStartTotalMinutes = minStartTime.Value.Hours * 60 + minStartTime.Value.Minutes;
                        if (slotStartTotalMinutes < minStartTotalMinutes) continue;
                    }
                    if (maxEndTime.HasValue)
                    {
                        var slotEndTotalMinutes = slot.EndTime.Hours * 60 + slot.EndTime.Minutes;
                        var maxEndTotalMinutes = maxEndTime.Value.Hours * 60 + maxEndTime.Value.Minutes;
                        if (slotEndTotalMinutes > maxEndTotalMinutes) continue;
                    }
                    
                    // Đảm bảo thời gian nghỉ 30 phút
                    bool hasEnoughGap = true;
                    foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                    {
                        if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                        {
                            var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                            var slotStart = slot.MatchDate.Date.Add(slot.StartTime);
                            var gapMinutes = (slotStart - usedMatchEnd).TotalMinutes;
                            if (gapMinutes < 30)
                            {
                                hasEnoughGap = false;
                                break;
                            }
                        }
                    }
                    if (!hasEnoughGap) continue;
                    
                    selectedSlot = slot;
                    slotIndex = j + 1;
                    Console.WriteLine($"   ⚠️ Round 1 Match {matchCount + 1}: Không tìm thấy slot rảnh, sử dụng slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} ({slot.MatchDate.DayOfWeek}) - Có thể conflict với lịch học");
                    break;
                }
            }
            
            if (selectedSlot == null)
            {
                Console.WriteLine($"   ⚠️ Không còn slot nào cho match {team1} vs {team2}");
                break; // Không còn slot nào
            }
            
            usedSlots.Add(selectedSlot);

                    var match = new AiActivityMatch
                    {
                        ActivityId = request.ActivityId,
                        SportId = request.SportId,
                        ClassGroup1Id = team1,
                        ClassGroup2Id = team2,
                        Grade = request.Grade,
                MatchDate = selectedSlot.MatchDate,
                StartTime = selectedSlot.StartTime,
                EndTime = selectedSlot.EndTime,
                Location = selectedSlot.Location,
                        Status = AiMatchStatus.Pending,
                Round = 1,
                RoundName = $"Vòng 1",
                        MatchNumber = matchNumber++,
                        IsBye = false,
                Notes = foundNonConflictSlot ? $"{teamType}" : $"{teamType} (Có thể conflict lịch học)"
                    };

            round1Matches.Add(match);
                    matches.Add(match);
            matchCount++;
        }
        
        // QUAN TRỌNG: Sắp xếp các matches vòng 1 theo thời gian để đảm bảo thứ tự
        round1Matches = round1Matches
            .OrderBy(m => m.MatchDate)
            .ThenBy(m => m.StartTime)
            .ToList();
        
        // Tạo matches cho các rounds tiếp theo (bao gồm cả bye teams)
        // Sử dụng cấu trúc để track teams/winners cho mỗi round
        var currentRoundParticipants = new List<(int? TeamId, int? PreviousMatchId, bool IsBye)>();
        
        // Thêm bye teams vào round 2
        foreach (var byeTeam in byeTeams)
        {
            currentRoundParticipants.Add((byeTeam, null, true));
        }
        
        // Thêm winners từ round 1
        foreach (var match in round1Matches)
        {
            currentRoundParticipants.Add((null, match.MatchNumber, false));
        }
        
        Console.WriteLine($"   🔄 {branchName}: Bắt đầu tạo matches cho các rounds tiếp theo (round 2 → {numberOfRounds})");
        Console.WriteLine($"      - Số participants cho round 2: {currentRoundParticipants.Count}");
        
        for (int round = 2; round <= numberOfRounds; round++)
        {
            // QUAN TRỌNG: Round cuối cùng của nhánh là "Bán kết"
            // "Chung kết" chỉ dùng cho match chung kết giữa 2 nhánh
            var roundName = round == numberOfRounds ? $"Bán kết" : $"Vòng {round}";
            var roundMatches = new List<AiActivityMatch>();
            
            // QUAN TRỌNG: Xác định round này có phải Bán kết hoặc Chung kết không
            // Bán kết và Chung kết chỉ dùng slots cuối tuần (Thứ 7, Chủ Nhật)
            bool isSemifinalOrFinal = roundName.Contains("Bán kết") || roundName.Contains("Chung kết");
            
            Console.WriteLine($"   🔄 {branchName}: Tạo Round {round} ({roundName}) - Số participants: {currentRoundParticipants.Count}");
            if (isSemifinalOrFinal)
            {
                Console.WriteLine($"   📅 Round {round} ({roundName}): Chỉ sử dụng slots cuối tuần (Thứ 7, Chủ Nhật)");
            }
            else
            {
                Console.WriteLine($"   📅 Round {round} ({roundName}): Ưu tiên slots ngày thường (Thứ 2 - Thứ 6), fallback cuối tuần nếu không có");
            }
            
            // Tạo matches cho round này
            var nextRoundParticipants = new List<(int? TeamId, int? PreviousMatchId, bool IsBye)>();
            
            // Lấy tất cả slots còn lại
            var allAvailableSlotsForRound = preferredSlots.Where(s => !usedSlotsCopy.Contains(s)).ToList();
            allAvailableSlotsForRound.AddRange(fallbackSlots.Where(s => !usedSlotsCopy.Contains(s)));
            
            // QUAN TRỌNG: Lọc slots theo loại round
            // Bán kết và Chung kết: chỉ dùng slots cuối tuần (Thứ 7, Chủ Nhật)
            // Các rounds khác: ưu tiên slots ngày thường, nhưng có thể dùng cuối tuần nếu không có
            if (isSemifinalOrFinal)
            {
                allAvailableSlotsForRound = allAvailableSlotsForRound
                    .Where(s => {
                        var dayOfWeek = s.MatchDate.DayOfWeek;
                        return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
                    })
                    .ToList();
                Console.WriteLine($"   📅 Round {round}: Đã lọc {allAvailableSlotsForRound.Count} slots cuối tuần");
            }
            else
            {
                // Ưu tiên slots ngày thường, nhưng vẫn giữ slots cuối tuần làm fallback
                var weekdaySlotsForRound = allAvailableSlotsForRound
                    .Where(s => {
                        var dayOfWeek = s.MatchDate.DayOfWeek;
                        return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
                    })
                    .ToList();
                
                var weekendSlotsForRound = allAvailableSlotsForRound
                    .Where(s => {
                        var dayOfWeek = s.MatchDate.DayOfWeek;
                        return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
                    })
                    .ToList();
                
                // Nếu có slots ngày thường, chỉ dùng slots ngày thường
                // Nếu không có, dùng slots cuối tuần
                if (weekdaySlotsForRound.Any())
                {
                    allAvailableSlotsForRound = weekdaySlotsForRound;
                    Console.WriteLine($"   📅 Round {round}: Ưu tiên {allAvailableSlotsForRound.Count} slots ngày thường (bỏ qua {weekendSlotsForRound.Count} slots cuối tuần)");
                }
                else
                {
                    allAvailableSlotsForRound = weekendSlotsForRound;
                    Console.WriteLine($"   ⚠️ Round {round}: Không có slots ngày thường, sử dụng {allAvailableSlotsForRound.Count} slots cuối tuần");
                }
            }
            
            // QUAN TRỌNG: Đảm bảo round sau diễn ra sau round trước
            // Tìm thời gian muộn nhất của các matches ở round trước
            var latestPreviousMatchTime = matches
                .Where(m => m.Round < round && m.MatchDate.HasValue && m.EndTime.HasValue)
                .Select(m => m.MatchDate.Value.Date.Add(m.EndTime.Value))
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();
            
            // Lọc slots: chỉ lấy slots sau thời gian muộn nhất của round trước + 30 phút nghỉ
            // QUAN TRỌNG: Phải đảm bảo thời gian nghỉ 30 phút giữa các trận
            var validSlotsForRound = allAvailableSlotsForRound
                .Where(s => {
                    var slotStartTime = s.MatchDate.Date.Add(s.StartTime);
                    // Phải sau round trước + 30 phút nghỉ
                    if (slotStartTime <= latestPreviousMatchTime.AddMinutes(30)) return false;
                    
                    // Kiểm tra ràng buộc thời gian (minStartTime, maxEndTime)
                    if (minStartTime.HasValue)
                    {
                        var slotStartTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
                        var minStartTotalMinutes = minStartTime.Value.Hours * 60 + minStartTime.Value.Minutes;
                        if (slotStartTotalMinutes < minStartTotalMinutes) return false;
                    }
                    if (maxEndTime.HasValue)
                    {
                        var slotEndTotalMinutes = s.EndTime.Hours * 60 + s.EndTime.Minutes;
                        var maxEndTotalMinutes = maxEndTime.Value.Hours * 60 + maxEndTime.Value.Minutes;
                        if (slotEndTotalMinutes > maxEndTotalMinutes) return false;
                    }
                    
                    return true;
                })
                .OrderBy(s => s.MatchDate)
                .ThenBy(s => s.StartTime)
                .ToList();
            
            // NGHIÊM NGẶT: Nếu không có slot nào sau round trước, báo lỗi và không tạo matches
            if (!validSlotsForRound.Any())
            {
                Console.WriteLine($"   ❌ Round {round}: KHÔNG CÓ slot nào sau round trước (muộn nhất: {latestPreviousMatchTime:yyyy-MM-dd HH:mm})");
                Console.WriteLine($"      - Số slots còn lại: {allAvailableSlotsForRound.Count}");
                if (allAvailableSlotsForRound.Any())
                {
                    var latestAvailableSlot = allAvailableSlotsForRound
                        .OrderByDescending(s => s.MatchDate)
                        .ThenByDescending(s => s.StartTime)
                        .First();
                    Console.WriteLine($"      - Slot muộn nhất có thể: {latestAvailableSlot.MatchDate:yyyy-MM-dd} {latestAvailableSlot.StartTime:hh\\:mm} (KHÔNG hợp lệ vì trước round trước)");
                }
                Console.WriteLine($"   ❌ KHÔNG THỂ tạo matches cho Round {round} - thiếu slots hợp lệ!");
                // KHÔNG tiếp tục - dừng tạo matches cho round này
                break; // Dừng vòng lặp, không tạo matches cho round này
            }
            
            // QUAN TRỌNG: Sắp xếp slots theo thứ tự thời gian
            // Bán kết và Chung kết: ưu tiên slots cuối tuần sớm nhất
            // Các rounds khác: ưu tiên slots ngày thường sớm nhất
            var sortedSlotsForRound = validSlotsForRound
                .OrderBy(s => s.MatchDate) // Ngày sớm nhất trước
                .ThenBy(s => s.StartTime) // Giờ sớm nhất trước
                .ThenBy(s => preferredSlots.Contains(s) ? 0 : 1) // Preferred slots trước
                .ToList();
            
            Console.WriteLine($"   📅 Round {round}: Slots được sắp xếp theo thứ tự thời gian (từ {sortedSlotsForRound.FirstOrDefault()?.MatchDate:yyyy-MM-dd} đến {sortedSlotsForRound.LastOrDefault()?.MatchDate:yyyy-MM-dd})");
            
            int matchCountForRound = 0;
            int slotIdx = 0;
            for (int i = 0; i < currentRoundParticipants.Count / 2 && matchCountForRound < currentRoundParticipants.Count / 2; i++)
            {
                var participant1 = currentRoundParticipants[i * 2];
                var participant2 = currentRoundParticipants[i * 2 + 1];
                
                // Tìm slot phù hợp cho 2 lớp này
                ScheduleSlot? selectedSlot = null;
                bool foundNonConflictSlot = false;
                
                // Lấy danh sách các lớp có thể tham gia match này (từ previous matches nếu chờ kết quả)
                // QUAN TRỌNG: Cần check conflict với lịch học của TẤT CẢ các lớp có thể tham gia
                // vì match này sẽ có 1 trong 2 lớp từ previous matches
                var possibleClassGroupIds = new HashSet<int>();
                
                // Lấy lớp từ participant 1
                if (participant1.TeamId.HasValue)
                {
                    possibleClassGroupIds.Add(participant1.TeamId.Value);
                }
                else if (participant1.PreviousMatchId.HasValue)
                {
                    // Tìm các lớp từ previous match (có thể là 1 trong 2 lớp)
                    var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant1.PreviousMatchId.Value);
                    if (prevMatch != null)
                    {
                        if (prevMatch.ClassGroup1Id.HasValue) possibleClassGroupIds.Add(prevMatch.ClassGroup1Id.Value);
                        if (prevMatch.ClassGroup2Id.HasValue) possibleClassGroupIds.Add(prevMatch.ClassGroup2Id.Value);
                    }
                }
                
                // Lấy lớp từ participant 2
                if (participant2.TeamId.HasValue)
                {
                    possibleClassGroupIds.Add(participant2.TeamId.Value);
                }
                else if (participant2.PreviousMatchId.HasValue)
                {
                    // Tìm các lớp từ previous match (có thể là 1 trong 2 lớp)
                    var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant2.PreviousMatchId.Value);
                    if (prevMatch != null)
                    {
                        if (prevMatch.ClassGroup1Id.HasValue) possibleClassGroupIds.Add(prevMatch.ClassGroup1Id.Value);
                        if (prevMatch.ClassGroup2Id.HasValue) possibleClassGroupIds.Add(prevMatch.ClassGroup2Id.Value);
                    }
                }
                
                // QUAN TRỌNG: Nếu match này phụ thuộc vào previous matches, phải diễn ra SAU previous matches
                DateTime? latestPreviousMatchEndTime = null;
                if (participant1.PreviousMatchId.HasValue)
                {
                    var prevMatch1 = matches.FirstOrDefault(m => m.MatchNumber == participant1.PreviousMatchId.Value);
                    if (prevMatch1 != null && prevMatch1.MatchDate.HasValue && prevMatch1.EndTime.HasValue)
                    {
                        var prevEndTime = prevMatch1.MatchDate.Value.Date.Add(prevMatch1.EndTime.Value);
                        if (!latestPreviousMatchEndTime.HasValue || prevEndTime > latestPreviousMatchEndTime.Value)
                        {
                            latestPreviousMatchEndTime = prevEndTime;
                        }
                    }
                }
                if (participant2.PreviousMatchId.HasValue)
                {
                    var prevMatch2 = matches.FirstOrDefault(m => m.MatchNumber == participant2.PreviousMatchId.Value);
                    if (prevMatch2 != null && prevMatch2.MatchDate.HasValue && prevMatch2.EndTime.HasValue)
                    {
                        var prevEndTime = prevMatch2.MatchDate.Value.Date.Add(prevMatch2.EndTime.Value);
                        if (!latestPreviousMatchEndTime.HasValue || prevEndTime > latestPreviousMatchEndTime.Value)
                        {
                            latestPreviousMatchEndTime = prevEndTime;
                        }
                    }
                }
                
                // Bước 1: Tìm slot không conflict với lịch học của các lớp có thể tham gia (ưu tiên)
                // QUAN TRỌNG: Slot PHẢI sau thời gian kết thúc của previous matches (nghiêm ngặt, không fallback)
                // QUAN TRỌNG: Phải lấp đầy slots theo thứ tự thời gian, không được bỏ qua
                for (int j = slotIdx; j < sortedSlotsForRound.Count; j++)
                {
                    var slot = sortedSlotsForRound[j];
                    if (usedSlotsCopy.Contains(slot)) continue;
                    
                    var slotStartTime = slot.MatchDate.Date.Add(slot.StartTime);
                    
                    // QUAN TRỌNG: Kiểm tra ràng buộc thời gian (minStartTime, maxEndTime)
                    if (minStartTime.HasValue)
                    {
                        var slotStartTotalMinutes = slot.StartTime.Hours * 60 + slot.StartTime.Minutes;
                        var minStartTotalMinutes = minStartTime.Value.Hours * 60 + minStartTime.Value.Minutes;
                        if (slotStartTotalMinutes < minStartTotalMinutes) continue;
                    }
                    if (maxEndTime.HasValue)
                    {
                        var slotEndTotalMinutes = slot.EndTime.Hours * 60 + slot.EndTime.Minutes;
                        var maxEndTotalMinutes = maxEndTime.Value.Hours * 60 + maxEndTime.Value.Minutes;
                        if (slotEndTotalMinutes > maxEndTotalMinutes) continue;
                    }
                    
                    // NGHIÊM NGẶT: Phải sau round trước + 30 phút nghỉ
                    if (slotStartTime <= latestPreviousMatchTime.AddMinutes(30))
                    {
                        continue; // Bỏ qua slot này vì không đủ 30 phút nghỉ sau round trước
                    }
                    
                    // NGHIÊM NGẶT: Phải sau previous matches (nếu có) + 30 phút nghỉ
                    if (latestPreviousMatchEndTime.HasValue)
                    {
                        if (slotStartTime <= latestPreviousMatchEndTime.Value.AddMinutes(30))
                        {
                            continue; // Bỏ qua slot này vì không đủ 30 phút nghỉ sau previous match
                        }
                    }
                    
                    // QUAN TRỌNG: Đảm bảo thời gian nghỉ 30 phút với tất cả matches đã tạo trong cùng ngày
                    bool hasEnoughGap = true;
                    foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                    {
                        if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                        {
                            var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                            var gapMinutes = (slotStartTime - usedMatchEnd).TotalMinutes;
                            if (gapMinutes < 30) // Cần ít nhất 30 phút nghỉ
                            {
                                hasEnoughGap = false;
                                break;
                            }
                        }
                    }
                    if (!hasEnoughGap) continue;
                    
                    // Check conflict với lịch học của tất cả các lớp có thể tham gia match này
                    // Vì match này sẽ có 1 trong các lớp này, nên cần check tất cả để đảm bảo không conflict
                    bool hasConflict = false;
                    foreach (var classGroupId in possibleClassGroupIds)
                    {
                        if (HasScheduleConflict(slot, classGroupId, null, schedulesByClassGroup))
                        {
                            hasConflict = true;
                            break;
                        }
                    }
                    
                    if (!hasConflict)
                    {
                        selectedSlot = slot;
                        foundNonConflictSlot = true;
                        slotIdx = j + 1;
                        break;
                    }
                }
                
                // Bước 2: Nếu không tìm thấy slot rảnh, chọn slot đầu tiên còn lại theo thứ tự thời gian
                // (không bắt buộc tất cả lớp phải rảnh, nhưng vẫn ưu tiên slot rảnh)
                // QUAN TRỌNG: Vẫn phải đảm bảo slot sau previous matches (nghiêm ngặt)
                // QUAN TRỌNG: Phải lấp đầy slots theo thứ tự thời gian, không được bỏ qua
                if (selectedSlot == null)
                {
                    for (int j = slotIdx; j < sortedSlotsForRound.Count; j++)
                    {
                        var slot = sortedSlotsForRound[j];
                        if (usedSlotsCopy.Contains(slot)) continue;
                        
                        var slotStartTime = slot.MatchDate.Date.Add(slot.StartTime);
                        
                        // Kiểm tra ràng buộc thời gian
                        if (minStartTime.HasValue)
                        {
                            var slotStartTotalMinutes = slot.StartTime.Hours * 60 + slot.StartTime.Minutes;
                            var minStartTotalMinutes = minStartTime.Value.Hours * 60 + minStartTime.Value.Minutes;
                            if (slotStartTotalMinutes < minStartTotalMinutes) continue;
                        }
                        if (maxEndTime.HasValue)
                        {
                            var slotEndTotalMinutes = slot.EndTime.Hours * 60 + slot.EndTime.Minutes;
                            var maxEndTotalMinutes = maxEndTime.Value.Hours * 60 + maxEndTime.Value.Minutes;
                            if (slotEndTotalMinutes > maxEndTotalMinutes) continue;
                        }
                        
                        // NGHIÊM NGẶT: Phải sau round trước + 30 phút nghỉ
                        if (slotStartTime <= latestPreviousMatchTime.AddMinutes(30))
                        {
                            continue; // Bỏ qua slot này vì không đủ 30 phút nghỉ sau round trước
                        }
                        
                        // NGHIÊM NGẶT: Phải sau previous matches (nếu có) + 30 phút nghỉ
                        if (latestPreviousMatchEndTime.HasValue)
                        {
                            if (slotStartTime <= latestPreviousMatchEndTime.Value.AddMinutes(30))
                            {
                                continue; // Bỏ qua slot này vì không đủ 30 phút nghỉ sau previous match
                            }
                        }
                        
                        // Đảm bảo thời gian nghỉ 30 phút với tất cả matches đã tạo trong cùng ngày
                        bool hasEnoughGap = true;
                        foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                        {
                            if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                            {
                                var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                                var gapMinutes = (slotStartTime - usedMatchEnd).TotalMinutes;
                                if (gapMinutes < 30)
                                {
                                    hasEnoughGap = false;
                                    break;
                                }
                            }
                        }
                        if (!hasEnoughGap) continue;
                        
                        selectedSlot = slot;
                        slotIdx = j + 1;
                        Console.WriteLine($"   ⚠️ Round {round} Match {i + 1}: Không tìm thấy slot rảnh cho tất cả lớp có thể tham gia, sử dụng slot có thể conflict");
                        break;
                    }
                }
                
                // NGHIÊM NGẶT: Nếu không tìm được slot sau previous matches, KHÔNG tạo match và báo lỗi
                if (selectedSlot == null)
                {
                    Console.WriteLine($"   ❌ {branchName} Round {round} Match {i + 1}: KHÔNG THỂ tìm slot nào sau previous matches!");
                    Console.WriteLine($"      - Thời gian muộn nhất của round trước: {latestPreviousMatchTime:yyyy-MM-dd HH:mm}");
                    if (latestPreviousMatchEndTime.HasValue)
                        Console.WriteLine($"      - Thời gian kết thúc của previous matches: {latestPreviousMatchEndTime.Value:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"      - Số slots còn lại: {sortedSlotsForRound.Count(s => !usedSlotsCopy.Contains(s))}");
                    Console.WriteLine($"      - Số matches đã tạo trong round này: {roundMatches.Count}");
                    Console.WriteLine($"      - Số matches còn cần tạo: {(currentRoundParticipants.Count / 2) - roundMatches.Count}");
                    Console.WriteLine($"   ❌ {branchName}: DỪNG tạo matches cho Round {round} - thiếu slots hợp lệ!");
                    break; // Dừng tạo matches cho round này
                }
                
                usedSlots.Add(selectedSlot);
                    
                    var match = new AiActivityMatch
                    {
                        ActivityId = request.ActivityId,
                        SportId = request.SportId,
                    ClassGroup1Id = participant1.TeamId,
                    ClassGroup2Id = participant2.TeamId,
                        Grade = request.Grade,
                    MatchDate = selectedSlot.MatchDate,
                    StartTime = selectedSlot.StartTime,
                    EndTime = selectedSlot.EndTime,
                    Location = selectedSlot.Location,
                        Status = AiMatchStatus.Pending,
                        Round = round,
                        RoundName = roundName,
                        MatchNumber = matchNumber++,
                    IsBye = participant1.IsBye || participant2.IsBye,
                    Notes = participant1.IsBye || participant2.IsBye 
                        ? $"Bye - {teamType}" 
                        : foundNonConflictSlot 
                            ? $"{teamType}" 
                            : $"{teamType} (Có thể conflict lịch học)"
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
                
                // Winner của match này sẽ vào round tiếp theo
                nextRoundParticipants.Add((null, match.MatchNumber, false));
            }
            
            // Nếu có đội lẻ (bye), thêm vào round tiếp theo
            if (currentRoundParticipants.Count % 2 == 1)
            {
                var remainingParticipant = currentRoundParticipants.Last();
                nextRoundParticipants.Add(remainingParticipant);
            }
            
            // QUAN TRỌNG: Sắp xếp các matches trong round này theo thời gian để đảm bảo thứ tự
            roundMatches = roundMatches
                .OrderBy(m => m.MatchDate)
                .ThenBy(m => m.StartTime)
                .ToList();
            
            Console.WriteLine($"   ✅ {branchName}: Đã tạo {roundMatches.Count} matches cho Round {round} ({roundName})");
            Console.WriteLine($"      - Số participants cho round tiếp theo: {nextRoundParticipants.Count}");
            
            currentRoundParticipants = nextRoundParticipants;
        }
        
        Console.WriteLine($"   ✅ {branchName}: Hoàn thành tạo matches - Tổng: {matches.Count} matches");
        
        return matches;
    }
    
    /// <summary>
    /// Tạo trận Chung kết/Bán kết giữa 2 nhánh
    /// </summary>
    /// <returns>Tuple (matches, requiresWeekend) - requiresWeekend = true nếu cần thêm ngày thứ 7, chủ nhật</returns>
    private (List<AiActivityMatch> matches, bool requiresWeekend) GenerateFinalMatchesBetweenBranches(
        TournamentScheduleRequest request,
        List<AiActivityMatch> morningBranchMatches,
        List<AiActivityMatch> afternoonBranchMatches,
        List<ScheduleSlot> allSlots,
        ref int matchNumber,
        ref HashSet<ScheduleSlot> usedSlots)
    {
        var matches = new List<AiActivityMatch>();
        bool requiresWeekend = false;
        
        // Tìm winners của mỗi nhánh (match có round cao nhất và không có NextMatchId)
        // QUAN TRỌNG: Winner là match có round cao nhất trong nhánh, không có NextMatchId
        // Nếu có nhiều matches cùng round cao nhất, chọn match có MatchNumber lớn nhất (match cuối cùng)
        var morningBranchWinner = morningBranchMatches
            .Where(m => m.NextMatchId == null && m.Round > 0)
            .OrderByDescending(m => m.Round)
            .ThenByDescending(m => m.MatchNumber)
            .FirstOrDefault();
            
        var afternoonBranchWinner = afternoonBranchMatches
            .Where(m => m.NextMatchId == null && m.Round > 0)
            .OrderByDescending(m => m.Round)
            .ThenByDescending(m => m.MatchNumber)
            .FirstOrDefault();
        
        Console.WriteLine($"   🔍 Tìm winners: Morning winner = {(morningBranchWinner != null ? $"Match {morningBranchWinner.MatchNumber} (Round {morningBranchWinner.Round})" : "null")}, Afternoon winner = {(afternoonBranchWinner != null ? $"Match {afternoonBranchWinner.MatchNumber} (Round {afternoonBranchWinner.Round})" : "null")}");
        
        // Kiểm tra nếu một trong hai nhánh không có matches hoặc không có winner
        if (morningBranchMatches.Count == 0 && afternoonBranchMatches.Count == 0)
        {
            Console.WriteLine($"   ⚠️ Không thể tạo chung kết: Cả hai nhánh đều không có matches");
            return (matches, false);
        }
        
        if (morningBranchWinner == null && afternoonBranchWinner == null)
        {
            // Nếu cả hai nhánh đều không có winner, không cần chung kết
            Console.WriteLine($"   ⚠️ Không thể tạo chung kết: Cả hai nhánh đều không có winner");
            return (matches, false);
        }
        
        // Nếu chỉ có 1 nhánh có winner, không cần chung kết (nhánh đó đã thắng)
        if (morningBranchWinner == null || afternoonBranchWinner == null)
        {
            Console.WriteLine($"   ℹ️ Chỉ có 1 nhánh có winner, không cần chung kết: Morning winner = {morningBranchWinner != null}, Afternoon winner = {afternoonBranchWinner != null}");
            Console.WriteLine($"   ⚠️ LƯU Ý: Nếu cả 2 nhánh đều có matches nhưng không tìm được winner, có thể do logic tìm winner sai!");
            if (morningBranchMatches.Any() && morningBranchWinner == null)
            {
                Console.WriteLine($"      - Nhánh Sáng có {morningBranchMatches.Count} matches nhưng không có winner");
                var morningMatchesWithoutNext = morningBranchMatches.Where(m => m.NextMatchId == null).ToList();
                Console.WriteLine($"      - Số matches không có NextMatchId: {morningMatchesWithoutNext.Count}");
                if (morningMatchesWithoutNext.Any())
                {
                    var maxRound = morningMatchesWithoutNext.Max(m => m.Round);
                    Console.WriteLine($"      - Round cao nhất: {maxRound}");
                }
            }
            if (afternoonBranchMatches.Any() && afternoonBranchWinner == null)
            {
                Console.WriteLine($"      - Nhánh Chiều có {afternoonBranchMatches.Count} matches nhưng không có winner");
                var afternoonMatchesWithoutNext = afternoonBranchMatches.Where(m => m.NextMatchId == null).ToList();
                Console.WriteLine($"      - Số matches không có NextMatchId: {afternoonMatchesWithoutNext.Count}");
                if (afternoonMatchesWithoutNext.Any())
                {
                    var maxRound = afternoonMatchesWithoutNext.Max(m => m.Round);
                    Console.WriteLine($"      - Round cao nhất: {maxRound}");
                }
            }
            return (matches, false);
        }
        
        // Copy usedSlots để dùng trong lambda (tránh lỗi ref trong lambda)
        var usedSlotsCopy = usedSlots;
        
        // QUAN TRỌNG: Chung kết phải diễn ra sau tất cả các trận khác
        // Tìm thời gian muộn nhất của tất cả matches đã tạo
        var allMatches = new List<AiActivityMatch>();
        allMatches.AddRange(morningBranchMatches);
        allMatches.AddRange(afternoonBranchMatches);
        
        var latestMatchTime = allMatches
            .Where(m => m.MatchDate.HasValue && m.EndTime.HasValue)
            .Select(m => m.MatchDate.Value.Date.Add(m.EndTime.Value))
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();
        
        // QUAN TRỌNG: Chung kết chờ kết quả từ morningBranchWinner và afternoonBranchWinner
        // Phải đảm bảo Chung kết diễn ra SAU cả 2 trận này
        DateTime? morningWinnerEndTime = null;
        if (morningBranchWinner.MatchDate.HasValue && morningBranchWinner.EndTime.HasValue)
        {
            morningWinnerEndTime = morningBranchWinner.MatchDate.Value.Date.Add(morningBranchWinner.EndTime.Value);
        }
        
        DateTime? afternoonWinnerEndTime = null;
        if (afternoonBranchWinner.MatchDate.HasValue && afternoonBranchWinner.EndTime.HasValue)
        {
            afternoonWinnerEndTime = afternoonBranchWinner.MatchDate.Value.Date.Add(afternoonBranchWinner.EndTime.Value);
        }
        
        // Thời gian muộn nhất = max của tất cả matches HOẶC 2 branch winners (nếu có)
        var finalLatestTime = latestMatchTime;
        if (morningWinnerEndTime.HasValue && morningWinnerEndTime.Value > finalLatestTime)
        {
            finalLatestTime = morningWinnerEndTime.Value;
        }
        if (afternoonWinnerEndTime.HasValue && afternoonWinnerEndTime.Value > finalLatestTime)
        {
            finalLatestTime = afternoonWinnerEndTime.Value;
        }
        
        Console.WriteLine($"   🏆 Chung kết: Thời gian muộn nhất của các trận khác = {finalLatestTime:yyyy-MM-dd HH:mm}");
        if (morningWinnerEndTime.HasValue)
            Console.WriteLine($"      - Morning winner kết thúc: {morningWinnerEndTime.Value:yyyy-MM-dd HH:mm}");
        if (afternoonWinnerEndTime.HasValue)
            Console.WriteLine($"      - Afternoon winner kết thúc: {afternoonWinnerEndTime.Value:yyyy-MM-dd HH:mm}");
        
        // QUAN TRỌNG: Trận chung kết giữa Nhóm A (đá chiều) vs Nhóm B (đá sáng)
        // Nhóm A chỉ đá được chiều (13:00-18:00), Nhóm B chỉ đá được sáng (07:00-11:30)
        // KHÔNG có crossover slots, nên cần tìm slot trong ngày thứ 7 hoặc chủ nhật
        // để cả 2 nhóm đều có thể tham gia (không bị ràng buộc bởi lịch học)
        
        // Tìm slot phù hợp cho chung kết: 
        // 1. Phải sau tất cả các trận khác VÀ sau cả 2 branch winners
        // 2. Phải là ngày thứ 7 hoặc chủ nhật (để cả 2 nhóm đều rảnh)
        // 3. Hoặc nếu không có ngày cuối tuần, yêu cầu thêm ngày cuối tuần
        var availableSlotsAfter = allSlots
            .Where(s => !usedSlotsCopy.Contains(s))
            .Where(s => {
                var slotStartTime = s.MatchDate.Date.Add(s.StartTime);
                // Phải sau tất cả matches + 30 phút nghỉ
                if (slotStartTime <= finalLatestTime.AddMinutes(30)) return false;
                // Phải sau morning winner (nếu có) + 30 phút nghỉ
                if (morningWinnerEndTime.HasValue && slotStartTime <= morningWinnerEndTime.Value.AddMinutes(30)) return false;
                // Phải sau afternoon winner (nếu có) + 30 phút nghỉ
                if (afternoonWinnerEndTime.HasValue && slotStartTime <= afternoonWinnerEndTime.Value.AddMinutes(30)) return false;
                
                // QUAN TRỌNG: Ưu tiên ngày thứ 7, chủ nhật (để cả 2 nhóm đều rảnh)
                var dayOfWeek = s.MatchDate.DayOfWeek;
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    // Ngày cuối tuần: có thể đá bất kỳ giờ nào (07:00-18:00, trừ khung giờ cấm 11:30-13:00)
                    var slotStartTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
                    var slotEndTotalMinutes = s.EndTime.Hours * 60 + s.EndTime.Minutes;
                    var forbiddenStartMinutes = 11 * 60 + 30; // 11:30
                    var forbiddenEndMinutes = 13 * 60; // 13:00
                    
                    // Không được overlap với khung giờ cấm
                    if (slotStartTotalMinutes < forbiddenEndMinutes && slotEndTotalMinutes > forbiddenStartMinutes)
                    {
                        return false;
                    }
                    
                    // Phải trong khoảng 07:00-18:00
                    if (slotStartTotalMinutes >= 7 * 60 && slotEndTotalMinutes <= 18 * 60)
                    {
                        return true;
                    }
                }
                
                return false; // Chỉ chấp nhận ngày cuối tuần
            })
            .ToList();
        
        ScheduleSlot? selectedSlot = null;
        
        if (availableSlotsAfter.Any())
        {
            // Chọn slot MUỘN NHẤT sau tất cả matches (không phải sớm nhất)
            selectedSlot = availableSlotsAfter
                .OrderByDescending(s => s.MatchDate)
                .ThenByDescending(s => s.StartTime)
                .First();
            Console.WriteLine($"   ✅ Chung kết: Chọn slot sau tất cả matches = {selectedSlot.MatchDate:yyyy-MM-dd} {selectedSlot.StartTime:hh\\:mm}");
        }
        else
        {
            // QUAN TRỌNG: Nếu không có slot phù hợp (ngày cuối tuần), yêu cầu thêm ngày thứ 7, chủ nhật
            Console.WriteLine($"   ❌ Chung kết: KHÔNG THỂ tìm slot phù hợp cho trận chung kết giữa 2 nhóm!");
            Console.WriteLine($"      - Morning winner kết thúc: {(morningWinnerEndTime.HasValue ? morningWinnerEndTime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}");
            Console.WriteLine($"      - Afternoon winner kết thúc: {(afternoonWinnerEndTime.HasValue ? afternoonWinnerEndTime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}");
            Console.WriteLine($"      - Tổng số slots còn lại: {allSlots.Count(s => !usedSlotsCopy.Contains(s))}");
            
            // Kiểm tra xem có slot nào trong ngày cuối tuần không
            var weekendSlots = allSlots
                .Where(s => !usedSlotsCopy.Contains(s))
                .Where(s => {
                    var dayOfWeek = s.MatchDate.DayOfWeek;
                    return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
                })
                .ToList();
            
            if (!weekendSlots.Any())
            {
                Console.WriteLine($"   ⚠️ YÊU CẦU: Cần thêm ngày thứ 7, chủ nhật để đảm bảo sức khỏe học sinh cho trận chung kết!");
                Console.WriteLine($"      - Lý do: Nhóm A (học sáng) chỉ đá được chiều (13:00-18:00)");
                Console.WriteLine($"      - Lý do: Nhóm B (học chiều) chỉ đá được sáng (07:00-11:30)");
                Console.WriteLine($"      - Giải pháp: Thêm ngày thứ 7, chủ nhật để cả 2 nhóm đều rảnh");
                requiresWeekend = true;
            }
            else
            {
                Console.WriteLine($"   ⚠️ Có {weekendSlots.Count} slots trong ngày cuối tuần, nhưng không phù hợp với thời gian yêu cầu");
                Console.WriteLine($"   ⚠️ YÊU CẦU: Cần thêm ngày thứ 7, chủ nhật với slots sau {finalLatestTime.AddMinutes(30):yyyy-MM-dd HH:mm}");
                requiresWeekend = true;
            }
        }
        
        if (selectedSlot != null)
        {
            var slot = selectedSlot;
            usedSlots.Add(slot);
            
            var finalMatch = new AiActivityMatch
                    {
                        ActivityId = request.ActivityId,
                        SportId = request.SportId,
                ClassGroup1Id = null, // Sẽ được cập nhật từ winner
                ClassGroup2Id = null, // Sẽ được cập nhật từ winner
                        Grade = request.Grade,
                        MatchDate = slot.MatchDate,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Location = slot.Location,
                        Status = AiMatchStatus.Pending,
                Round = Math.Max(morningBranchWinner.Round, afternoonBranchWinner.Round) + 1,
                RoundName = "Chung kết",
                        MatchNumber = matchNumber++,
                        IsBye = false,
                Notes = "Chung kết giữa 2 nhánh"
            };
            
            // Link previous matches
            morningBranchWinner.NextMatchId = finalMatch.MatchNumber;
            afternoonBranchWinner.NextMatchId = finalMatch.MatchNumber;
            
            matches.Add(finalMatch);
        }
        else if (requiresWeekend)
        {
            Console.WriteLine($"   ⚠️ KHÔNG tạo match chung kết vì cần thêm ngày thứ 7, chủ nhật");
        }

        return (matches, requiresWeekend);
    }
    
    // Helper classes cho conflict detection

    private List<AiActivityMatch> GenerateRoundRobinMatches(
        TournamentScheduleRequest request,
        List<ScheduleSlot> slots,
        List<int> classGroupIds,
        ref int matchNumber,
        ClassGroupScheduleAnalysis? scheduleAnalysis = null)
    {
        var matches = new List<AiActivityMatch>();
        
        // Phân loại lớp và slots (tương tự Single Elimination)
        var morningClasses = scheduleAnalysis?.MorningClasses ?? new HashSet<int>();
        var afternoonClasses = scheduleAnalysis?.AfternoonClasses ?? new HashSet<int>();
        
        var morningSlots = slots.Where(s => {
            var hour = s.StartTime.Hours;
            return hour >= 6 && hour < 12;
        }).ToList();
        
        var afternoonSlots = slots.Where(s => {
            var hour = s.StartTime.Hours;
            return hour >= 12 && hour < 18;
        }).ToList();
        
        var otherSlots = slots.Where(s => {
            var hour = s.StartTime.Hours;
            return !(hour >= 6 && hour < 18);
        }).ToList();
        
        int morningSlotIndex = 0;
        int afternoonSlotIndex = 0;
        int otherSlotIndex = 0;

        // Round robin: mỗi team đấu với tất cả teams khác
        // Ưu tiên: lớp buổi sáng đấu với nhau vào buổi chiều, lớp buổi chiều đấu với nhau vào buổi sáng
        for (int i = 0; i < classGroupIds.Count; i++)
        {
            for (int j = i + 1; j < classGroupIds.Count; j++)
            {
                var team1 = classGroupIds[i];
                var team2 = classGroupIds[j];
                
                bool team1IsMorning = morningClasses.Contains(team1);
                bool team2IsMorning = morningClasses.Contains(team2);
                bool team1IsAfternoon = afternoonClasses.Contains(team1);
                bool team2IsAfternoon = afternoonClasses.Contains(team2);
                
                ScheduleSlot? slot = null;
                
                // Logic: Nếu cả 2 đều buổi sáng → dùng afternoon slot
                // Nếu cả 2 đều buổi chiều → dùng morning slot
                // Nếu khác buổi hoặc chưa xác định → dùng afternoon slot (ưu tiên)
                if (team1IsMorning && team2IsMorning)
                {
                    // Cả 2 đều buổi sáng → dùng afternoon slot
                    if (afternoonSlotIndex < afternoonSlots.Count)
                    {
                        slot = afternoonSlots[afternoonSlotIndex++];
                    }
                    else if (morningSlotIndex < morningSlots.Count)
                    {
                        slot = morningSlots[morningSlotIndex++];
                    }
                    else if (otherSlotIndex < otherSlots.Count)
                    {
                        slot = otherSlots[otherSlotIndex++];
                    }
                }
                else if (team1IsAfternoon && team2IsAfternoon)
                {
                    // Cả 2 đều buổi chiều → dùng morning slot
                    if (morningSlotIndex < morningSlots.Count)
                    {
                        slot = morningSlots[morningSlotIndex++];
                    }
                    else if (afternoonSlotIndex < afternoonSlots.Count)
                    {
                        slot = afternoonSlots[afternoonSlotIndex++];
                    }
                    else if (otherSlotIndex < otherSlots.Count)
                    {
                        slot = otherSlots[otherSlotIndex++];
                    }
                }
                else
                {
                    // Khác buổi hoặc chưa xác định → ưu tiên afternoon slot
                    if (afternoonSlotIndex < afternoonSlots.Count)
                    {
                        slot = afternoonSlots[afternoonSlotIndex++];
                    }
                    else if (morningSlotIndex < morningSlots.Count)
                    {
                        slot = morningSlots[morningSlotIndex++];
                    }
                    else if (otherSlotIndex < otherSlots.Count)
                    {
                        slot = otherSlots[otherSlotIndex++];
                    }
                }
                
                if (slot == null)
                {
                    break; // Không còn slot nào
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

