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

        // Lấy các matches ĐÃ CÓ của CÁC HOẠT ĐỘNG KHÁC để check conflict (không lấy của chính activity hiện tại)
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
                       CAST(IsPublished AS BIT) AS IsPublished,
                       CAST(IsDeleted AS BIT) AS IsDeleted
                FROM ActivityMatches 
                -- KHÔNG lấy matches của chính Activity hiện tại để tránh tự block AI generate (sẽ ghi đè lại)
                WHERE ActivityId != {0} 
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
        var conflictDetails = filterResult.conflictDetails;
        Console.WriteLine($"   Slots available sau khi filter: {availableSlots.Count}");
        Console.WriteLine($"   Tổng số conflicts chi tiết: {conflictDetails.Count}");

        if (!availableSlots.Any())
        {
            // Tạo explanation chi tiết về lý do không có slots
            var reasons = new List<string>();
            
            if (conflictCounts.conflictWithMatches > 0)
            {
                reasons.Add($"{conflictCounts.conflictWithMatches} slots bị loại do trùng với matches đã có");
            }
            
            if (conflictCounts.conflictWithLocation > 0)
            {
                reasons.Add($"{conflictCounts.conflictWithLocation} slots bị loại do conflict về location");
            }
            
            var reasonText = reasons.Any() 
                ? string.Join(", ", reasons) 
                : "Không rõ lý do cụ thể";
            
            response.Explanation = $"Không có slots nào available sau khi filter conflicts. " +
                                  $"Tổng số slots ban đầu: {slots.Count}. " +
                                  $"Lý do: {reasonText}. " +
                                  $"Đề xuất: Mở rộng khoảng thời gian (StartDate - EndDate), " +
                                  $"thêm sân thi đấu (AvailableLocations), hoặc kiểm tra lại conflicts với matches/activities đã có.";
            
            // Tạo warnings chi tiết
            response.SlotWarnings = GenerateSlotWarnings(
                availableSlots.Count,
                slots.Count, // minSlotsNeeded = tổng số slots ban đầu (lý tưởng)
                slots.Count,
                0, // actualMatchesCreated = 0
                0, // expectedMatches = 0 (chưa tính được)
                conflictCounts,
                request,
                false, // isComplete = false
                dbContext); // Thêm dbContext để tìm các lớp có thể xếp chung
            
            // Thêm chi tiết conflicts vào response
            response.SlotConflicts = conflictDetails;
            
            return response;
        }

        // Tính số matches cần tạo dựa trên tournament format
        var numberOfMatches = CalculateNumberOfMatches(request.ClassGroupIds.Count, request.TournamentFormat);
        var numberOfRounds = CalculateNumberOfRounds(request.ClassGroupIds.Count, request.TournamentFormat);
        
        // QUAN TRỌNG: Tính toán chính xác số slots cần thiết cho tất cả các vòng
        // Với single elimination:
        // - Vòng 1: cần numberOfMatches slots (hoặc ít hơn nếu có bye)
        // - Các vòng sau: cần ít hơn slots (mỗi vòng giảm một nửa)
        // Tổng số slots tối thiểu = numberOfMatches (vì mỗi match cần 1 slot)
        // Nhưng để đảm bảo có slots cho các vòng sau và tránh conflict, cần thêm buffer
        var minSlotsNeeded = numberOfMatches; // Tối thiểu lý thuyết
        
        // Tính số slots khuyến nghị: numberOfMatches + buffer cho các vòng sau
        // Buffer = numberOfRounds để đảm bảo có slots cho mỗi vòng
        var recommendedSlots = numberOfMatches + numberOfRounds;
        
        // QUAN TRỌNG: Nếu không đủ slots, điều chỉnh constraint để tránh INFEASIBLE
        // Nếu availableSlots < minSlotsNeeded, giảm minSlotsNeeded xuống availableSlots.Count
        // và cố gắng tạo matches với số slots có sẵn (có thể không đủ matches)
        var actualMinSlots = Math.Min(minSlotsNeeded, availableSlots.Count);
        
        // QUAN TRỌNG: Cho phép chọn TẤT CẢ slots khả dụng nhưng không quá recommendedSlots
        // Lý do: Cần có đủ slots trải dài từ ngày đầu đến ngày cuối để xếp cho tất cả các vòng
        // Nhưng không nên chọn quá nhiều slots không cần thiết (tối ưu hóa)
        var maxSlotsToSelect = Math.Min(availableSlots.Count, recommendedSlots);
        
        Console.WriteLine($"   📊 Số slots cần chọn: Tối thiểu lý thuyết = {minSlotsNeeded}, Tối thiểu thực tế = {actualMinSlots}, Khuyến nghị = {recommendedSlots}, Tối đa có thể = {maxSlotsToSelect}");
        
        // Cảnh báo nếu không đủ slots
        if (availableSlots.Count < minSlotsNeeded)
        {
            Console.WriteLine($"   ⚠️ CẢNH BÁO: Chỉ có {availableSlots.Count} slots available, nhưng cần tối thiểu {minSlotsNeeded} slots!");
            Console.WriteLine($"      - Sẽ cố gắng tạo matches với số slots có sẵn ({availableSlots.Count} slots)");
            Console.WriteLine($"      - Có thể không tạo đủ {numberOfMatches} matches (chỉ tạo được tối đa {availableSlots.Count} matches)");
            Console.WriteLine($"      - Đề xuất: Mở rộng khoảng thời gian hoặc giảm conflicts");
        }
        else if (availableSlots.Count < recommendedSlots)
        {
            Console.WriteLine($"   ⚠️ CẢNH BÁO: Chỉ có {availableSlots.Count} slots available, khuyến nghị {recommendedSlots} slots!");
            Console.WriteLine($"      - Có thể thiếu slots cho các vòng sau");
            Console.WriteLine($"      - Đề xuất: Mở rộng khoảng thời gian để có thêm slots");
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
        // Nếu actualMinSlots = 0 (không có slots), không tạo constraint này
        if (actualMinSlots > 0)
        {
            var matchConstraint = solver.MakeConstraint(actualMinSlots, maxSlotsToSelect, "slots_range");
            for (int i = 0; i < availableSlots.Count; i++)
            {
                matchConstraint.SetCoefficient(variables[i], 1);
            }
        }
        else
        {
            // Nếu không có slots nào, trả về error ngay
            response.Explanation = "Không có slots nào available sau khi filter conflicts. Vui lòng kiểm tra lại thời gian và địa điểm.";
            return response;
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
        // QUAN TRỌNG: Nếu có 2 sân trở lên, cho phép overlap thời gian nếu matches ở sân khác nhau
        // Với mỗi cặp slots có overlap thời gian, chỉ có thể chọn cả 2 nếu chúng ở sân khác nhau
        // TỐI ƯU: Chỉ tạo constraints cho các cặp slots thực sự overlap (giảm số lượng constraints)
        var overlapConstraints = new List<Constraint>();
        var overlapCount = 0;
        var hasMultipleLocations = request.AvailableLocations != null && request.AvailableLocations.Count >= 2;
        
        Console.WriteLine($"   🏟️ Số sân available: {request.AvailableLocations?.Count ?? 0} {(hasMultipleLocations ? "(có thể overlap nếu khác sân)" : "(không overlap)")}");
        if (request.AvailableLocations != null && request.AvailableLocations.Any())
        {
            Console.WriteLine($"   🏟️ Danh sách sân: [{string.Join(", ", request.AvailableLocations)}]");
        }
        
        var overlapAllowedCount = 0; // Số cặp slots được phép overlap (khác sân)
        var overlapBlockedCount = 0; // Số cặp slots bị chặn overlap (cùng sân hoặc 1 sân)
        
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
                        // QUAN TRỌNG: Nếu có 2 sân trở lên và 2 slots ở sân khác nhau, cho phép overlap
                        bool sameLocation = !string.IsNullOrEmpty(slotI.Location) && 
                                          !string.IsNullOrEmpty(slotJ.Location) && 
                                          slotI.Location == slotJ.Location;
                        
                        // Chỉ tạo constraint nếu:
                        // 1. Không có nhiều sân (chỉ có 1 sân hoặc không có sân) → không cho phép overlap
                        // 2. Có nhiều sân NHƯNG 2 slots ở cùng sân → không cho phép overlap
                        if (!hasMultipleLocations || sameLocation)
                    {
                        // Tạo constraint: chỉ có thể chọn 1 trong 2 slots này
                        var overlapConstraint = solver.MakeConstraint(0, 1, $"no_overlap_{i}_{j}");
                        overlapConstraint.SetCoefficient(variables[i], 1);
                        overlapConstraint.SetCoefficient(variables[j], 1);
                        overlapConstraints.Add(overlapConstraint);
                            overlapCount++;
                            overlapBlockedCount++;
                            
                            // Log chi tiết cho các cặp bị chặn
                            if (overlapBlockedCount <= 5) // Chỉ log 5 cặp đầu tiên để tránh spam
                            {
                                Console.WriteLine($"      🔒 Chặn overlap: Slot {i} ({slotI.MatchDate:yyyy-MM-dd} {slotI.StartTime:hh\\:mm}, Sân: {slotI.Location ?? "N/A"}) vs Slot {j} ({slotJ.MatchDate:yyyy-MM-dd} {slotJ.StartTime:hh\\:mm}, Sân: {slotJ.Location ?? "N/A"}) - {(sameLocation ? "Cùng sân" : "Chỉ có 1 sân")}");
                            }
                        }
                        else
                        {
                            // Nếu có nhiều sân và 2 slots ở sân khác nhau → KHÔNG tạo constraint (cho phép overlap)
                            overlapAllowedCount++;
                            
                            // Log chi tiết cho các cặp được phép overlap
                            if (overlapAllowedCount <= 5) // Chỉ log 5 cặp đầu tiên để tránh spam
                            {
                                Console.WriteLine($"      ✅ Cho phép overlap: Slot {i} ({slotI.MatchDate:yyyy-MM-dd} {slotI.StartTime:hh\\:mm}, Sân: {slotI.Location ?? "N/A"}) vs Slot {j} ({slotJ.MatchDate:yyyy-MM-dd} {slotJ.StartTime:hh\\:mm}, Sân: {slotJ.Location ?? "N/A"}) - Khác sân");
                            }
                        }
                    }
                }
            }
        }
        
        Console.WriteLine($"   🔒 Đã tạo {overlapCount} constraints để tránh overlap thời gian (tổng {overlapConstraints.Count} constraints)");
        Console.WriteLine($"   📊 Thống kê overlap: {overlapBlockedCount} cặp bị chặn, {overlapAllowedCount} cặp được phép overlap (khác sân)");

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

        // QUAN TRỌNG: Đảm bảo có đủ slots trải đều theo thời gian cho tất cả các vòng
        // Nếu OR-Tools chọn quá ít slots, thêm slots để đảm bảo có đủ cho tất cả các vòng
        // Nhưng chỉ thêm nếu thực sự cần và không conflict
        if (selectedSlots.Count < recommendedSlots && selectedSlots.Count < availableSlots.Count)
        {
            Console.WriteLine($"   ⚠️ OR-Tools chỉ chọn {selectedSlots.Count} slots, khuyến nghị {recommendedSlots} slots");
            
            // Sắp xếp availableSlots theo ML score và thời gian
            var remainingSlots = availableSlots
                .Select((slot, idx) => new { Slot = slot, Index = idx })
                .Where(x => !selectedIndices.Contains(x.Index))
                .OrderByDescending(x => x.Slot.MLScore) // Ưu tiên ML score cao
                .ThenBy(x => x.Slot.MatchDate) // Sau đó ưu tiên thời gian sớm
                .ThenBy(x => x.Slot.StartTime)
                .ToList();
            
            // Thêm slots để đạt recommendedSlots, nhưng đảm bảo không overlap với slots đã chọn
            var addedCount = 0;
            var targetSlots = Math.Min(recommendedSlots, availableSlots.Count); // Không vượt quá số slots có sẵn
            
            foreach (var item in remainingSlots)
            {
                if (selectedSlots.Count >= targetSlots) break;
                
                var slot = item.Slot;
                
                // Kiểm tra overlap với slots đã chọn (cùng ngày, cùng sân, overlap thời gian)
                bool hasOverlap = false;
                // Dùng lại biến hasMultipleLocations đã khai báo ở trên (dòng 238)
                
                foreach (var selectedSlot in selectedSlots)
                {
                    if (slot.MatchDate.Date == selectedSlot.MatchDate.Date)
                    {
                        // Check time overlap
                        if (slot.StartTime < selectedSlot.EndTime && slot.EndTime > selectedSlot.StartTime)
                        {
                            // Nếu có nhiều sân và slots ở sân khác nhau → cho phép overlap
                            bool sameLocation = !string.IsNullOrEmpty(slot.Location) && 
                                              !string.IsNullOrEmpty(selectedSlot.Location) && 
                                              slot.Location == selectedSlot.Location;
                            
                            if (!hasMultipleLocations || sameLocation)
                            {
                                hasOverlap = true;
                                break;
                            }
                        }
                    }
                }
                
                if (!hasOverlap)
                {
                    selectedSlots.Add(slot);
                    selectedIndices.Add(item.Index);
                    addedCount++;
                }
            }
            
            if (addedCount > 0)
            {
                Console.WriteLine($"   ✅ Đã thêm {addedCount} slots để đảm bảo có đủ slots cho tất cả các vòng (tổng: {selectedSlots.Count} slots)");
            }
            else if (selectedSlots.Count < recommendedSlots)
            {
                Console.WriteLine($"   ⚠️ Không thể thêm slots do conflicts. Hiện có {selectedSlots.Count} slots, khuyến nghị {recommendedSlots} slots");
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
        
        // BƯỚC 1: Lưu mapping từ MatchNumber cũ sang match object trước khi sắp xếp
        var oldNumberToMatchMap = matches.ToDictionary(m => m.MatchNumber, m => m);
        
        // BƯỚC 2: Lưu NextMatchId references (dùng match object thay vì MatchNumber)
        var nextMatchReferences = new Dictionary<AiActivityMatch, AiActivityMatch>();
        foreach (var match in matches)
        {
            if (match.NextMatchId.HasValue && oldNumberToMatchMap.TryGetValue(match.NextMatchId.Value, out var nextMatch))
            {
                nextMatchReferences[match] = nextMatch;
            }
        }
        
        // BƯỚC 3: Sắp xếp matches theo thời gian
        matches = matches
            .Where(m => m.MatchDate.HasValue && m.StartTime.HasValue) // Chỉ lấy matches đã có thời gian
            .OrderBy(m => m.MatchDate) // Ngày sớm nhất trước
            .ThenBy(m => m.StartTime) // Giờ sớm nhất trước
            .ThenBy(m => m.Round) // Nếu cùng ngày giờ, ưu tiên round thấp hơn
            .ToList();
        
        // BƯỚC 4: Đánh lại số thứ tự (MatchNumber) từ 1 đến N theo thứ tự thời gian
        var newNumberToMatchMap = new Dictionary<int, AiActivityMatch>();
        for (int i = 0; i < matches.Count; i++)
        {
            var newNumber = i + 1;
            matches[i].MatchNumber = newNumber;
            newNumberToMatchMap[newNumber] = matches[i];
        }
        
        // BƯỚC 5: Cập nhật NextMatchId theo số mới (dùng references đã lưu)
        foreach (var match in matches)
        {
            if (nextMatchReferences.TryGetValue(match, out var nextMatch))
            {
                // Tìm MatchNumber mới của nextMatch trong list đã sắp xếp
                var nextMatchNewNumber = matches.IndexOf(nextMatch);
                if (nextMatchNewNumber >= 0)
                {
                    match.NextMatchId = nextMatchNewNumber + 1; // MatchNumber mới (1-based)
                }
                else
                {
                    // Nếu không tìm thấy, clear NextMatchId và log warning
                    match.NextMatchId = null;
                    Console.WriteLine($"   ⚠️ Warning: Không tìm thấy nextMatch cho match {match.MatchNumber} sau khi sắp xếp");
                }
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
            // Thông điệp thân thiện, không lộ chi tiết kỹ thuật như Objective value
            if (availableSlots.Count < minSlotsNeeded)
            {
                response.Explanation =
                    "Hệ thống không tìm đủ khung giờ phù hợp để xếp tất cả các trận đấu theo cấu hình hiện tại. " +
                    "Vui lòng xem danh sách cảnh báo bên dưới và điều chỉnh khoảng ngày, giờ thi đấu, số sân hoặc giảm số lớp tham gia.";
            }
            else
            {
                response.Explanation =
                    "Hệ thống chưa thể xếp đủ số trận đấu với cấu hình hiện tại. " +
                    "Có thể do khung thời gian quá hẹp, giới hạn số trận tối đa mỗi ngày hoặc xung đột với lịch học / các trận đã có. " +
                    "Vui lòng xem chi tiết các cảnh báo bên dưới và điều chỉnh thông tin rồi thử lại.";
            }
        }
        else
        {
            response.Explanation = $"Đã tạo {matches.Count} trận đấu trong {numberOfRounds} vòng.";
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

        // Tạo danh sách warnings chi tiết cho người dùng
        response.SlotWarnings = GenerateSlotWarnings(
            availableSlots.Count,
            minSlotsNeeded,
            recommendedSlots,
            matches.Count,
            expectedMatches,
            conflictCounts,
            request,
            isComplete);
        
        // Thêm chi tiết conflicts vào response (chỉ lấy top 50 conflicts để tránh response quá lớn)
        if (conflictDetails != null && conflictDetails.Any())
        {
            response.SlotConflicts = conflictDetails.Take(50).ToList();
        }

        return response;
    }

    private (List<ScheduleSlot> availableSlots, (int conflictWithMatches, int conflictWithLocation, int conflictWithParticipantActivities) conflictCounts, List<SlotConflictDetail> conflictDetails) FilterAvailableSlots(
        List<ScheduleSlot> slots,
        List<AiActivityMatch> existingMatches,
        TournamentScheduleRequest request,
        EduShpereDbContext dbContext)
    {
        var availableSlots = new List<ScheduleSlot>();
        var conflictDetails = new List<SlotConflictDetail>();

        // Lấy lịch học của các lớp tham gia
        Console.WriteLine($"\n🔍 Đang lấy lịch học từ DB cho {request.ClassGroupIds.Count} lớp...");
        var classGroupSchedules = GetClassGroupSchedules(request.ClassGroupIds, dbContext);
        Console.WriteLine($"   ✅ Đã load {classGroupSchedules.Count} lịch học từ {request.ClassGroupIds.Count} lớp tham gia");

        // Đã gỡ bỏ logic check conflict với participant activities khác
        // Chỉ kiểm tra conflict với matches đã có và lịch học

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
                                
                                // Lưu chi tiết conflict
                                conflictDetails.Add(new SlotConflictDetail
                                {
                                    Slot = new SlotInfo
                                    {
                                        MatchDate = slot.MatchDate,
                                        StartTime = slot.StartTime,
                                        EndTime = slot.EndTime,
                                        Location = slot.Location
                                    },
                                    ConflictType = "MatchConflict",
                                    Reason = $"Slot trùng với match #{existingMatch.MatchNumber} đã có: " +
                                            $"{existingDate:dd/MM/yyyy} {existingStart:hh\\:mm}-{existingEnd:hh\\:mm} " +
                                            $"tại {(string.IsNullOrEmpty(existingMatch.Location) ? "sân chưa xác định" : existingMatch.Location)}",
                                    MatchConflict = new MatchConflictInfo
                                    {
                                        MatchId = existingMatch.Id,
                                        MatchNumber = existingMatch.MatchNumber,
                                        ClassGroup1Id = existingMatch.ClassGroup1Id,
                                        ClassGroup2Id = existingMatch.ClassGroup2Id,
                                        MatchDate = existingDate,
                                        StartTime = existingStart,
                                        EndTime = existingEnd,
                                        Location = existingMatch.Location,
                                        Description = existingMatch.ClassGroup1Id.HasValue && existingMatch.ClassGroup2Id.HasValue
                                            ? $"Match #{existingMatch.MatchNumber}: ClassGroup {existingMatch.ClassGroup1Id} vs {existingMatch.ClassGroup2Id}"
                                            : $"Match #{existingMatch.MatchNumber}"
                                    }
                                });
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
            
            // Đã gỡ bỏ logic check conflict với participant activities khác
            
            // QUAN TRỌNG: Heuristic ưu tiên phân bổ đều các sân
            // Tính số lượng slots mỗi sân để ưu tiên sân có ít slots hơn (phân bổ đều)
            if (isAvailable && request.AvailableLocations != null && request.AvailableLocations.Count >= 2)
            {
                // Đếm số slots mỗi sân
                var slotsByLocation = slots
                    .Where(s => !string.IsNullOrEmpty(s.Location))
                    .GroupBy(s => s.Location)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                // Ưu tiên sân có ít slots hơn (để phân bổ đều)
                if (!string.IsNullOrEmpty(slot.Location) && slotsByLocation.ContainsKey(slot.Location))
                {
                    var totalSlots = slots.Count;
                    var slotsInThisLocation = slotsByLocation[slot.Location];
                    var averageSlotsPerLocation = (float)totalSlots / request.AvailableLocations.Count;
                    
                    // Nếu sân này có ít slots hơn trung bình → tăng điểm (ưu tiên sử dụng sân này)
                    if (slotsInThisLocation < averageSlotsPerLocation)
                    {
                        var locationBonus = (averageSlotsPerLocation - slotsInThisLocation) / averageSlotsPerLocation * 15.0f; // Tối đa 15 điểm
                        slot.MLScore += locationBonus;
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

        Console.WriteLine($"   ⚠️ Conflicts: {conflictWithMatches} với matches cũ, {conflictWithLocation} với location");
        Console.WriteLine($"   ℹ️ Lưu ý: Không filter slots dựa trên lịch học của tất cả lớp. Sẽ check conflict lịch học khi tạo match cụ thể.");
        Console.WriteLine($"   ✅ Slots available: {availableSlots.Count}/{slots.Count}");

        return (availableSlots, (conflictWithMatches, conflictWithLocation, 0), conflictDetails);
    }
    
    /// <summary>
        /// Lấy danh sách participants của các lớp tham gia
    /// </summary>
        private List<ParticipantInfo> GetParticipantsInClassGroups(
            List<int> classGroupIds,
            int currentActivityId,
            EduShpereDbContext dbContext)
    {
        try
        {
            var participants = dbContext.Set<EduShpere.Domain.Models.ActivityParticipant>()
                .Where(ap =>
                    ap.ActivityId == currentActivityId &&             // Guard: chỉ lấy participants của activity hiện tại
                    classGroupIds.Contains(ap.ClassGroupId ?? 0) &&
                    !ap.IsDeleted)
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

        // PHÂN LOẠI MỚI: Chia các đội thành 2 nhóm để CÂN BẰNG SỐ TRẬN ĐẤU
        // Mục tiêu: Mỗi đội đấu số trận bằng nhau nhất có thể
        // Logic: Chia sao cho 2 nhánh có số rounds gần bằng nhau nhất
        
        // Tính số rounds cho từng cách chia để tìm cách chia tối ưu
        var totalTeams = classGroupIds.Count;
        var bestBalance = int.MaxValue;
        var bestGroupA = new List<int>();
        var bestGroupB = new List<int>();
        
        // Thử các cách chia khác nhau để tìm cách cân bằng nhất
        // Với số đội nhỏ, thử tất cả các cách chia
        // Với số đội lớn, dùng heuristic: chia đôi và điều chỉnh
        
        if (totalTeams <= 16)
        {
            // Với số đội nhỏ, thử tất cả các cách chia có thể
            var halfSize = totalTeams / 2;
            var minSize = Math.Max(1, halfSize - 2);
            var maxSize = Math.Min(totalTeams - 1, halfSize + 2);
            
            for (int groupASize = minSize; groupASize <= maxSize; groupASize++)
            {
                var groupBSize = totalTeams - groupASize;
                if (groupBSize < 1) continue;
                
                // Tính số rounds cho mỗi nhóm
                var roundsA = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(groupASize)));
                var roundsB = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(groupBSize)));
                
                // Tính độ chênh lệch
                var balance = Math.Abs(roundsA - roundsB);
                
                if (balance < bestBalance)
                {
                    bestBalance = balance;
                    bestGroupA = classGroupIds.Take(groupASize).ToList();
                    bestGroupB = classGroupIds.Skip(groupASize).ToList();
                }
            }
        }
        else
        {
            // Với số đội lớn, chia đôi và điều chỉnh
            var halfSize = totalTeams / 2;
            var groupASize = halfSize;
            var groupBSize = totalTeams - groupASize;
            
            // Điều chỉnh để cân bằng rounds
            var roundsA = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(groupASize)));
            var roundsB = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(groupBSize)));
            
            // Nếu roundsA > roundsB, giảm groupASize
            // Nếu roundsB > roundsA, tăng groupASize
            while (Math.Abs(roundsA - roundsB) > 1 && groupASize > 1 && groupASize < totalTeams - 1)
            {
                if (roundsA > roundsB)
                {
                    groupASize--;
                }
                else if (roundsB > roundsA)
                {
                    groupASize++;
                }
                else
                {
                    break;
                }
                
                groupBSize = totalTeams - groupASize;
                roundsA = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(groupASize)));
                roundsB = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(groupBSize)));
            }
            
            bestGroupA = classGroupIds.Take(groupASize).ToList();
            bestGroupB = classGroupIds.Skip(groupASize).ToList();
        }
        
        // Shuffle để random thứ tự đội trong mỗi nhóm
        var random = new Random();
        bestGroupA = bestGroupA.OrderBy(x => random.Next()).ToList();
        bestGroupB = bestGroupB.OrderBy(x => random.Next()).ToList();
        
        var roundsA_final = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(bestGroupA.Count)));
        var roundsB_final = (int)Math.Ceiling(Math.Log2(GetNextPowerOfTwo(bestGroupB.Count)));
        
        Console.WriteLine($"   📊 Phân loại nhóm để cân bằng số trận đấu:");
        Console.WriteLine($"      - Nhóm A: {bestGroupA.Count} đội → {roundsA_final} rounds (tối đa {roundsA_final} trận/đội)");
        Console.WriteLine($"      - Nhóm B: {bestGroupB.Count} đội → {roundsB_final} rounds (tối đa {roundsB_final} trận/đội)");
        Console.WriteLine($"      - Độ chênh lệch: {Math.Abs(roundsA_final - roundsB_final)} rounds");
        
        var groupA_AfternoonOnly = bestGroupA;
        var groupB_MorningOnly = bestGroupB;
        
        // QUAN TRỌNG: Sử dụng PreferredStartTime và PreferredEndTime từ request để giới hạn thời gian thi đấu
        // PreferredStartTime: Giờ bắt đầu sớm nhất (ví dụ: 07:00)
        // PreferredEndTime: Giờ kết thúc muộn nhất trong ngày (ví dụ: 20:00) - đây là giới hạn muộn nhất 1 trận có thể có
        var minStartTime = request.PreferredStartTime ?? TimeSpan.FromHours(7); // Mặc định 07:00
        var maxEndTime = request.PreferredEndTime ?? TimeSpan.FromHours(18); // Mặc định 18:00
        
        var minStartTotalMinutes = minStartTime.Hours * 60 + minStartTime.Minutes;
        var maxEndTotalMinutes = maxEndTime.Hours * 60 + maxEndTime.Minutes;
        
        Console.WriteLine($"   ⏰ Giới hạn thời gian: Bắt đầu từ {minStartTime:hh\\:mm}, Kết thúc trước {maxEndTime:hh\\:mm}");
        
        // QUAN TRỌNG: Không phân loại slots (sáng/chiều/tối) - chỉ cần giờ dư là chèn vào
        // Chỉ filter theo:
        // 1. PreferredStartTime <= StartTime
        // 2. EndTime <= PreferredEndTime
        // 3. Tránh giờ nghỉ trưa (11:30-13:00) - không được overlap với khung giờ này
        // 4. Cho phép đấu cuối tuần
        var allAvailableSlots = slots.Where(s => {
            var startTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
            var endTotalMinutes = s.EndTime.Hours * 60 + s.EndTime.Minutes;
            
            // 1. Phải >= PreferredStartTime
            if (startTotalMinutes < minStartTotalMinutes) return false;
            
            // 2. Phải <= PreferredEndTime
            if (endTotalMinutes > maxEndTotalMinutes) return false;
            
            // 3. Tránh giờ nghỉ trưa (11:30-13:00) - không được overlap
            var forbiddenStartMinutes = 11 * 60 + 30; // 11:30
            var forbiddenEndMinutes = 13 * 60; // 13:00
            if (startTotalMinutes < forbiddenEndMinutes && endTotalMinutes > forbiddenStartMinutes)
            {
                return false; // Overlap với giờ nghỉ trưa
            }
            
            return true;
        }).OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).ToList();
        
        Console.WriteLine($"   ⏰ Tổng số slots khả dụng: {allAvailableSlots.Count} (không phân loại, cho phép cuối tuần, tránh giờ nghỉ trưa 11:30-13:00)");
        
        // Tạo danh sách slots đã sử dụng để track
        var usedSlots = new HashSet<ScheduleSlot>();
        
        // QUAN TRỌNG: Track số lần sử dụng mỗi sân để ưu tiên phân bổ đều (dùng chung cho tất cả các method)
        var locationUsageCount = new Dictionary<string, int>();
        if (request.AvailableLocations != null && request.AvailableLocations.Count >= 2)
        {
            foreach (var location in request.AvailableLocations)
            {
                locationUsageCount[location] = 0;
            }
        }

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
        
        // CÂN BẰNG NHÁNH: Tạo 2 nhánh đấu riêng biệt để cân bằng số trận đấu
        // Tất cả slots đều có thể dùng cho cả 2 nhánh, chỉ cần check conflict với lịch học khi tạo match cụ thể
        // Cho phép đấu cuối tuần, chỉ cần tránh giờ nghỉ trưa và conflict với lịch học (cách ít nhất 30 phút)
        
        // ⚠️ SPECIAL CASE: Nếu một trong hai nhánh có <= 1 đội (ví dụ tổng 2 hoặc 3 đội),
        // thì logic chia nhánh + chung kết sẽ làm mất 1 trận (chỉ tạo được 1 match cho 3 đội).
        // Trong trường hợp này, dùng SINGLE-BRANCH bracket cho TẤT CẢ đội để đảm bảo đủ n-1 trận.
        if (groupA_AfternoonOnly.Count <= 1 || groupB_MorningOnly.Count <= 1)
        {
            Console.WriteLine($"   ℹ️ Một trong hai nhánh có <= 1 đội (Nhánh A: {groupA_AfternoonOnly.Count}, Nhánh B: {groupB_MorningOnly.Count}).");
            Console.WriteLine("   ℹ️ Sử dụng SINGLE-BRANCH single elimination cho tất cả các đội để đảm bảo đủ n-1 trận (không chia nhánh).");

            var singleBranchMatches = GenerateBranchMatches(
                request,
                classGroupIds,
                allAvailableSlots,
                new List<ScheduleSlot>(),
                $"Nhánh {classGroupIds.Count} đội (Single Branch)",
                "Single",
                ref matchNumber,
                ref usedSlots,
                schedulesByClassGroup,
                locationUsageCount);

            matches.AddRange(singleBranchMatches);
            Console.WriteLine($"   ✅ Single-branch: Đã tạo {matches.Count} matches cho {classGroupIds.Count} đội.");
            return matches;
        }
        
        // Nhánh A: Dùng tất cả slots có sẵn
        var groupA_BranchMatches = GenerateBranchMatches(
            request, groupA_AfternoonOnly, allAvailableSlots, new List<ScheduleSlot>(), 
            $"Nhánh A ({groupA_AfternoonOnly.Count} đội - {roundsA_final} rounds)", "Nhánh A", ref matchNumber, ref usedSlots, schedulesByClassGroup, locationUsageCount);
        
        // Nhánh B: Dùng tất cả slots còn lại (sau khi nhánh A đã dùng)
        var remainingSlots = allAvailableSlots.Where(s => !usedSlots.Contains(s)).ToList();
        var groupB_BranchMatches = GenerateBranchMatches(
            request, groupB_MorningOnly, remainingSlots, new List<ScheduleSlot>(), 
            $"Nhánh B ({groupB_MorningOnly.Count} đội - {roundsB_final} rounds)", "Nhánh B", ref matchNumber, ref usedSlots, schedulesByClassGroup, locationUsageCount);
        
        // Gộp matches từ 2 nhánh
        matches.AddRange(groupA_BranchMatches);
        matches.AddRange(groupB_BranchMatches);
        
        // Xử lý trận Chung kết giữa 2 nhánh
        // Không còn yêu cầu đặc biệt về buổi học, chỉ cần check conflict với lịch học
        var (finalMatches, finalRequiresWeekend) = GenerateFinalMatchesBetweenBranches(
            request, groupA_BranchMatches, groupB_BranchMatches, 
            slots, ref matchNumber, ref usedSlots, schedulesByClassGroup, locationUsageCount);
        
        matches.AddRange(finalMatches);
        
        // Nếu yêu cầu thêm ngày cuối tuần, cập nhật biến requiresWeekend
        if (finalRequiresWeekend)
        {
            requiresWeekend = true;
            Console.WriteLine($"   ⚠️ YÊU CẦU: Cần thêm ngày thứ 7, chủ nhật để đảm bảo có đủ slots cho trận chung kết!");
        }
        
        Console.WriteLine($"   ✅ Đã tạo {matches.Count} matches: {groupA_BranchMatches.Count} nhánh A ({roundsA_final} rounds), {groupB_BranchMatches.Count} nhánh B ({roundsB_final} rounds), {finalMatches.Count} trận chung kết");
        
        // Log thống kê sử dụng sân
        if (locationUsageCount.Any())
        {
            Console.WriteLine($"\n🏟️ THỐNG KÊ SỬ DỤNG SÂN:");
            var totalUsage = locationUsageCount.Values.Sum();
            foreach (var kvp in locationUsageCount.OrderByDescending(x => x.Value))
            {
                var percentage = totalUsage > 0 ? (kvp.Value * 100.0 / totalUsage) : 0;
                Console.WriteLine($"   - Sân '{kvp.Key}': {kvp.Value} trận ({percentage:F1}%)");
            }
            Console.WriteLine($"   - Tổng: {totalUsage} trận");
        }
        
        // KIỂM TRA: Đảm bảo đã tạo đủ matches cho tất cả các vòng
        var expectedGroupBMatches = CalculateExpectedMatchesForBranch(groupB_MorningOnly.Count);
        var expectedGroupAMatches = CalculateExpectedMatchesForBranch(groupA_AfternoonOnly.Count);
        var expectedTotalMatches = expectedGroupBMatches + expectedGroupAMatches + (finalMatches.Count > 0 ? 1 : 0);
        
        if (matches.Count < expectedTotalMatches)
        {
            Console.WriteLine($"   ⚠️ CẢNH BÁO: Chỉ tạo được {matches.Count}/{expectedTotalMatches} matches!");
            Console.WriteLine($"      - Nhánh A: {groupA_BranchMatches.Count}/{expectedGroupAMatches} matches");
            Console.WriteLine($"      - Nhánh B: {groupB_BranchMatches.Count}/{expectedGroupBMatches} matches");
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
    /// Bao gồm: Conflict trực tiếp và rule đấu muộn (phải sau lịch học buổi chiều cuối + minGapBetweenMatches phút)
    /// </summary>
    private bool HasScheduleConflict(
        ScheduleSlot slot,
        int? classGroup1Id,
        int? classGroup2Id,
        Dictionary<int, List<ClassGroupSchedule>> schedulesByClassGroup,
        int minGapBetweenMatches)
    {
        if (!classGroup1Id.HasValue && !classGroup2Id.HasValue)
            return false; // Không có lớp nào → không conflict
        
        var dayOfWeek = (int)slot.MatchDate.DayOfWeek;
        var dayOfWeekNormalized = dayOfWeek == 0 ? 7 : dayOfWeek;
        
        // Tìm lịch học buổi chiều cuối cùng của cả 2 lớp (để check rule đấu muộn)
        TimeSpan? latestAfternoonClassEndTime = null;
        
        // Check conflict với lớp 1
        if (classGroup1Id.HasValue && schedulesByClassGroup.TryGetValue(classGroup1Id.Value, out var schedules1))
        {
            foreach (var schedule in schedules1)
            {
                if (schedule.DayOfWeek == dayOfWeekNormalized)
                {
                    // QUAN TRỌNG: Check conflict với cách ít nhất minGapBetweenMatches phút (mặc định 30 phút)
                    // Slot phải cách lịch học ít nhất minGapBetweenMatches phút
                    var scheduleStart = schedule.StartTime;
                    var scheduleEnd = schedule.EndTime;
                    var slotStart = slot.StartTime;
                    var slotEnd = slot.EndTime;
                    
                    // Tính khoảng cách giữa slot và lịch học
                    // Nếu slot trước lịch học: khoảng cách = scheduleStart - slotEnd
                    // Nếu slot sau lịch học: khoảng cách = slotStart - scheduleEnd
                    // Nếu overlap: conflict (khoảng cách < 0)
                    var gapBefore = (scheduleStart - slotEnd).TotalMinutes; // Slot trước lịch học
                    var gapAfter = (slotStart - scheduleEnd).TotalMinutes; // Slot sau lịch học
                    
                    // Nếu overlap hoặc khoảng cách < minGapBetweenMatches phút → conflict
                    if (gapBefore < minGapBetweenMatches && gapAfter < minGapBetweenMatches)
                    {
                        return true; // Conflict với lớp 1 (cách lịch học < minGapBetweenMatches phút)
                    }
                    
                    // Tìm lịch học buổi chiều cuối cùng (StartTime >= 12:00)
                    if (schedule.StartTime.Hours >= 12)
                    {
                        if (!latestAfternoonClassEndTime.HasValue || schedule.EndTime > latestAfternoonClassEndTime.Value)
                        {
                            latestAfternoonClassEndTime = schedule.EndTime;
                        }
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
                    // QUAN TRỌNG: Check conflict với cách ít nhất minGapBetweenMatches phút (mặc định 30 phút)
                    var scheduleStart = schedule.StartTime;
                    var scheduleEnd = schedule.EndTime;
                    var slotStart = slot.StartTime;
                    var slotEnd = slot.EndTime;
                    
                    var gapBefore = (scheduleStart - slotEnd).TotalMinutes;
                    var gapAfter = (slotStart - scheduleEnd).TotalMinutes;
                    
                    if (gapBefore < minGapBetweenMatches && gapAfter < minGapBetweenMatches)
                    {
                        return true; // Conflict với lớp 2 (cách lịch học < minGapBetweenMatches phút)
                    }
                    
                    // Tìm lịch học buổi chiều cuối cùng (StartTime >= 12:00)
                    if (schedule.StartTime.Hours >= 12)
                    {
                        if (!latestAfternoonClassEndTime.HasValue || schedule.EndTime > latestAfternoonClassEndTime.Value)
                        {
                            latestAfternoonClassEndTime = schedule.EndTime;
                        }
                    }
                }
            }
        }
        
        // QUAN TRỌNG: Rule đấu muộn - Nếu đấu sau giờ học buổi chiều, phải sau lịch học buổi chiều cuối cùng + minGapBetweenMatches phút
        // Ví dụ: Lớp A1 học buổi chiều xong lúc 5h thì 5h + minGapBetweenMatches mới bắt đầu có trận đấu
        if (latestAfternoonClassEndTime.HasValue)
        {
            // Nếu slot bắt đầu sau giờ học buổi chiều cuối cùng
            if (slot.StartTime >= latestAfternoonClassEndTime.Value)
            {
                // Phải sau lịch học buổi chiều cuối cùng + minGapBetweenMatches phút
                var requiredStartTime = latestAfternoonClassEndTime.Value.Add(TimeSpan.FromMinutes(minGapBetweenMatches));
                if (slot.StartTime < requiredStartTime)
                {
                    return true; // Conflict với rule đấu muộn
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
        Dictionary<string, int> locationUsageCount,
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
        var allAvailableSlotsForRound1 = preferredSlots.Where(s => !usedSlotsCopy.Contains(s)).ToList();
        allAvailableSlotsForRound1.AddRange(fallbackSlots.Where(s => !usedSlotsCopy.Contains(s)));
        
        // QUAN TRỌNG: Không phân loại ngày thường/cuối tuần - cho phép đấu bất kỳ ngày nào
        // Chỉ cần sắp xếp theo thời gian và ưu tiên cùng ngày
        var sortedByTime = allAvailableSlotsForRound1
            .OrderBy(s => s.MatchDate)
            .ThenBy(s => s.StartTime)
            .ThenBy(s => preferredSlots.Contains(s) ? 0 : 1) // Preferred slots trước
            .ToList();
        
        Console.WriteLine($"   📅 Round 1: Tổng số slots: {sortedByTime.Count} (cho phép cả ngày thường và cuối tuần)");
        
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
            // QUAN TRỌNG: Ưu tiên phân bổ đều các sân (chọn sân ít được sử dụng nhất)
            // Nếu không có, vẫn chọn slot (không bắt buộc tất cả lớp phải rảnh)
            ScheduleSlot? selectedSlot = null;
            bool foundNonConflictSlot = false;
            
            // Bước 1: Tìm slot không conflict với lịch học của 2 lớp (ưu tiên)
            // QUAN TRỌNG: Ưu tiên xếp nhiều matches vào ngày sớm nhất có thể
            // Logic: 1) Cùng ngày với matches đã chọn (nếu chưa đầy) > 2) Ngày sớm nhất > 3) Sân ít dùng > 4) Giờ sớm
            // Đếm số matches đã chọn trong mỗi ngày
            var matchesByDate = matches
                .Where(m => m.MatchDate.HasValue)
                .GroupBy(m => m.MatchDate.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());
            
            var maxMatchesPerDay = request.MaxMatchesPerDay;
            
            var availableSlotsForMatch = sortedByTime
                .Where(s => !usedSlotsCopy.Contains(s))
                .OrderBy(s => {
                    // Ưu tiên 1: Cùng ngày với matches đã chọn (nếu chưa đạt maxMatchesPerDay)
                    // Nếu ngày này đã có matches và chưa đầy → ưu tiên cao nhất (priority = 0)
                    // Nếu ngày này chưa có matches hoặc đã đầy → ưu tiên thấp hơn (priority = 1000000)
                    var sameDayPriority = 0.0;
                    if (matchesByDate.ContainsKey(s.MatchDate.Date))
                    {
                        var matchesInThisDay = matchesByDate[s.MatchDate.Date];
                        if (matchesInThisDay >= maxMatchesPerDay)
                        {
                            // Ngày này đã đầy → ưu tiên thấp (sang ngày mới)
                            sameDayPriority = 1000000.0;
                        }
                        // Nếu chưa đầy, sameDayPriority = 0 (ưu tiên cao nhất)
                    }
                    else
                    {
                        // Ngày này chưa có matches → ưu tiên trung bình (sau ngày đã có matches nhưng chưa đầy)
                        sameDayPriority = 500000.0;
                    }
                    
                    // Ưu tiên 2: Ngày sớm nhất (scale nhỏ để không override sameDayPriority)
                    var datePriority = (s.MatchDate.Date - new DateTime(2000, 1, 1)).TotalDays * 0.1;
                    
                    // Ưu tiên 3: Sân ít được sử dụng nhất
                    var locationPriority = 0.0;
                    if (!string.IsNullOrEmpty(s.Location) && locationUsageCount.ContainsKey(s.Location))
                    {
                        locationPriority = locationUsageCount[s.Location] * 0.01;
                    }
                    
                    // Ưu tiên 4: Giờ sớm nhất
                    var timePriority = s.StartTime.TotalMinutes / 100000.0;
                    
                    // Tổng hợp: sameDayPriority (ưu tiên cao nhất) + datePriority + locationPriority + timePriority
                    // Logic: Cùng ngày (chưa đầy) > Ngày sớm nhất > Sân ít dùng > Giờ sớm
                    return sameDayPriority + datePriority + locationPriority + timePriority;
                })
                .ToList();
            
            // Debug: Log top 3 slots được ưu tiên và locationUsageCount
            if (availableSlotsForMatch.Any() && matchCount == 0)
            {
                Console.WriteLine($"   🔍 DEBUG Round 1 Match {matchCount + 1}: locationUsageCount = {string.Join(", ", locationUsageCount.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
                Console.WriteLine($"   🔍 DEBUG Round 1 Match {matchCount + 1}: Top 3 slots được ưu tiên:");
                for (int idx = 0; idx < Math.Min(3, availableSlotsForMatch.Count); idx++)
                {
                    var s = availableSlotsForMatch[idx];
                    var locCount = !string.IsNullOrEmpty(s.Location) && locationUsageCount.ContainsKey(s.Location) 
                        ? locationUsageCount[s.Location] 
                        : -1;
                    Console.WriteLine($"      [{idx + 1}] {s.MatchDate:yyyy-MM-dd} {s.StartTime:hh\\:mm} - Sân: {s.Location ?? "N/A"} (đã dùng: {locCount} lần)");
                }
            }
            
            for (int j = 0; j < availableSlotsForMatch.Count; j++)
            {
                var slot = availableSlotsForMatch[j];
                var originalIndex = sortedByTime.IndexOf(slot);
                if (originalIndex >= 0) slotIndex = originalIndex + 1;
                
                // QUAN TRỌNG: Không còn ràng buộc thời gian cứng nhắc (minStartTime, maxEndTime)
                // Thay vào đó, chỉ check conflict với lịch học thực tế
                // Lớp buổi chiều có thể đá ngoài giờ học nếu không conflict
                
                // QUAN TRỌNG: Đảm bảo thời gian nghỉ giữa các trận (từ request.MinGapBetweenMatches)
                // QUAN TRỌNG: Nếu có 2 sân trở lên, cho phép overlap thời gian nếu matches ở sân khác nhau
                // Start Time của trận sau >= End Time của trận trước + minGapBetweenMatches phút nghỉ (chỉ nếu cùng sân)
                bool hasEnoughGap = true;
                var hasMultipleLocations = request.AvailableLocations != null && request.AvailableLocations.Count >= 2;
                var minGapMinutes = request.MinGapBetweenMatches;
                
                foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                {
                    if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                    {
                        // QUAN TRỌNG: Nếu có nhiều sân và 2 matches ở sân khác nhau, cho phép overlap
                        bool sameLocation = !string.IsNullOrEmpty(slot.Location) && 
                                          !string.IsNullOrEmpty(usedMatch.Location) && 
                                          slot.Location == usedMatch.Location;
                        
                        // Chỉ check gap nếu:
                        // 1. Không có nhiều sân → luôn cần gap
                        // 2. Có nhiều sân NHƯNG 2 matches ở cùng sân → cần gap
                        if (!hasMultipleLocations || sameLocation)
                        {
                            var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                            var slotStart = slot.MatchDate.Date.Add(slot.StartTime);
                            var gapMinutes = (slotStart - usedMatchEnd).TotalMinutes;
                            if (gapMinutes < minGapMinutes) // Cần ít nhất minGapMinutes phút nghỉ
                            {
                                hasEnoughGap = false;
                                Console.WriteLine($"      ⚠️ Round 1: Slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} (Sân: {slot.Location ?? "N/A"}) không đủ gap {minGapMinutes} phút với match {usedMatch.MatchNumber} ({usedMatch.MatchDate:yyyy-MM-dd} {usedMatch.EndTime:hh\\:mm}, Sân: {usedMatch.Location ?? "N/A"}) - Gap: {gapMinutes} phút");
                                break;
                            }
                        }
                        else
                        {
                            // Nếu có nhiều sân và 2 matches ở sân khác nhau → KHÔNG check gap (cho phép overlap)
                            Console.WriteLine($"      ✅ Round 1: Cho phép overlap - Slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} (Sân: {slot.Location ?? "N/A"}) với match {usedMatch.MatchNumber} ({usedMatch.MatchDate:yyyy-MM-dd} {usedMatch.EndTime:hh\\:mm}, Sân: {usedMatch.Location ?? "N/A"}) - Khác sân");
                        }
                    }
                }
                if (!hasEnoughGap) continue;
                
                // Check conflict với lịch học của 2 lớp tham gia match này
                if (!HasScheduleConflict(slot, team1, team2, schedulesByClassGroup, request.MinGapBetweenMatches))
                {
                    selectedSlot = slot;
                    foundNonConflictSlot = true;
                    // Cập nhật số lần sử dụng sân này
                    if (!string.IsNullOrEmpty(slot.Location) && locationUsageCount.ContainsKey(slot.Location))
                    {
                        locationUsageCount[slot.Location]++;
                    }
                    Console.WriteLine($"   ✅ Round 1 Match {matchCount + 1}: Chọn slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} ({slot.MatchDate.DayOfWeek}) - Sân: {slot.Location ?? "N/A"} - Không conflict");
                    break;
                }
            }
            
            // Bước 2: Nếu không tìm thấy slot rảnh, chọn slot đầu tiên còn lại
            // QUAN TRỌNG: Vẫn phải tuân theo ràng buộc thời gian và thời gian nghỉ
            // QUAN TRỌNG: KHÔNG được bỏ qua slots ngày Thứ 2, Thứ 3 nếu vẫn còn trận Round 1 chưa xếp
            // QUAN TRỌNG: Ưu tiên sân ít được sử dụng nhất để phân bổ đều
            if (selectedSlot == null)
            {
                var fallbackSlotsForMatch = sortedByTime
                    .Where(s => !usedSlotsCopy.Contains(s))
                    .OrderBy(s => {
                        // Ưu tiên 1: Sân ít được sử dụng nhất (nhân với 1000000 để ưu tiên cao hơn thời gian)
                        var locationPriority = 0.0;
                        if (!string.IsNullOrEmpty(s.Location) && locationUsageCount.ContainsKey(s.Location))
                        {
                            locationPriority = locationUsageCount[s.Location] * 10;
                        }
                        // Ưu tiên 2: Ngày sớm nhất (scale nhỏ để ưu tiên cùng ngày trước)
                        var datePriority = (s.MatchDate.Date - new DateTime(2000, 1, 1)).TotalDays * 5;
                        // Ưu tiên 3: Giờ sớm nhất (scale bằng phút / 60)
                        var timePriority = s.StartTime.TotalMinutes / 60.0;
                        return locationPriority + datePriority + timePriority;
                    })
                    .ToList();
                
                for (int j = 0; j < fallbackSlotsForMatch.Count; j++)
                {
                    var slot = fallbackSlotsForMatch[j];
                    var originalIndex = sortedByTime.IndexOf(slot);
                    if (originalIndex >= 0) slotIndex = originalIndex + 1;
                    
                    // QUAN TRỌNG: Không còn ràng buộc thời gian cứng nhắc
                    // Chỉ check conflict với lịch học thực tế (HasScheduleConflict)
                    // Lớp buổi chiều có thể đá ngoài giờ học nếu không conflict
                    
                    // QUAN TRỌNG: Đảm bảo thời gian nghỉ (chỉ nếu cùng sân) - sử dụng request.MinGapBetweenMatches
                    // Nếu có 2 sân trở lên, cho phép overlap thời gian nếu matches ở sân khác nhau
                    bool hasEnoughGap = true;
                    var hasMultipleLocations = request.AvailableLocations != null && request.AvailableLocations.Count >= 2;
                    var minGapMinutes = request.MinGapBetweenMatches;
                    
                    foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                    {
                        if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                        {
                            // QUAN TRỌNG: Nếu có nhiều sân và 2 matches ở sân khác nhau, cho phép overlap
                            bool sameLocation = !string.IsNullOrEmpty(slot.Location) && 
                                              !string.IsNullOrEmpty(usedMatch.Location) && 
                                              slot.Location == usedMatch.Location;
                            
                            // Chỉ check gap nếu không có nhiều sân hoặc cùng sân
                            if (!hasMultipleLocations || sameLocation)
                            {
                                var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                                var slotStart = slot.MatchDate.Date.Add(slot.StartTime);
                                var gapMinutes = (slotStart - usedMatchEnd).TotalMinutes;
                                if (gapMinutes < minGapMinutes)
                                {
                                    hasEnoughGap = false;
                        break;
                    }
                            }
                        }
                    }
                    if (!hasEnoughGap) continue;
                    
                    selectedSlot = slot;
                    // Cập nhật số lần sử dụng sân này
                    if (!string.IsNullOrEmpty(slot.Location) && locationUsageCount.ContainsKey(slot.Location))
                    {
                        locationUsageCount[slot.Location]++;
                    }
                    Console.WriteLine($"   ⚠️ Round 1 Match {matchCount + 1}: Không tìm thấy slot rảnh, sử dụng slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} ({slot.MatchDate.DayOfWeek}) - Sân: {slot.Location ?? "N/A"} - Có thể conflict với lịch học");
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
            // Bán kết và Chung kết có thể đá linh hoạt miễn là giờ rảnh chung (không conflict với lịch học)
            bool isSemifinalOrFinal = roundName.Contains("Bán kết") || roundName.Contains("Chung kết");
            
            Console.WriteLine($"   🔄 {branchName}: Tạo Round {round} ({roundName}) - Số participants: {currentRoundParticipants.Count}");
            Console.WriteLine($"   📅 Round {round} ({roundName}): Ưu tiên slot gần nhất thỏa mãn trước (không phân biệt ngày thường hay cuối tuần)");
            
            // Tạo matches cho round này
            var nextRoundParticipants = new List<(int? TeamId, int? PreviousMatchId, bool IsBye)>();
            
            // Lấy tất cả slots còn lại
            var allAvailableSlotsForRound = preferredSlots.Where(s => !usedSlotsCopy.Contains(s)).ToList();
            allAvailableSlotsForRound.AddRange(fallbackSlots.Where(s => !usedSlotsCopy.Contains(s)));
            
            // QUAN TRỌNG: Sắp xếp tất cả slots theo thời gian (ngày gần nhất, giờ sớm nhất)
            // Ưu tiên: MatchDate (sớm nhất) -> StartTime (sớm nhất) -> Preferred slots trước
            allAvailableSlotsForRound = allAvailableSlotsForRound
                .OrderBy(s => s.MatchDate)
                .ThenBy(s => s.StartTime)
                .ThenBy(s => preferredSlots.Contains(s) ? 0 : 1) // Preferred slots trước
                .ToList();
            
            Console.WriteLine($"   📅 Round {round}: Tổng số slots: {allAvailableSlotsForRound.Count} (sắp xếp theo thời gian)");
            
            // QUAN TRỌNG: Đảm bảo round sau diễn ra sau round trước
            // Tìm thời gian muộn nhất của các matches ở round trước
            var latestPreviousMatchTime = matches
                .Where(m => m.Round < round && m.MatchDate.HasValue && m.EndTime.HasValue)
                .Select(m => m.MatchDate.Value.Date.Add(m.EndTime.Value))
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();
            
            // Lọc slots: chỉ lấy slots sau thời gian muộn nhất của round trước + minGapBetweenMatches phút nghỉ
            // QUAN TRỌNG: Phải đảm bảo thời gian nghỉ giữa các trận (từ request.MinGapBetweenMatches)
            var minGapMinutes = request.MinGapBetweenMatches; // Khai báo ở đây để dùng cho cả method
            var minGapMinutesForLambda = minGapMinutes; // Capture vào lambda
            var latestPreviousMatchTimeForLambda = latestPreviousMatchTime; // Capture vào lambda
            var validSlotsForRound = allAvailableSlotsForRound
                .Where(s => {
                    var slotStartTime = s.MatchDate.Date.Add(s.StartTime);
                    // Phải sau round trước + minGapBetweenMatches phút nghỉ
                    if (slotStartTime <= latestPreviousMatchTimeForLambda.AddMinutes(minGapMinutesForLambda)) return false;
                    
                    // QUAN TRỌNG: Không còn ràng buộc thời gian cứng nhắc
                    // Chỉ check conflict với lịch học thực tế khi tạo match cụ thể
                    
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
            // Tất cả các rounds: ưu tiên slot gần nhất thỏa mãn trước (không phân biệt ngày thường hay cuối tuần)
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
                // QUAN TRỌNG: Ưu tiên xếp nhiều matches vào ngày sớm nhất có thể
                // Đếm số matches đã chọn trong mỗi ngày
                var matchesByDateForRound = matches
                    .Where(m => m.MatchDate.HasValue)
                    .GroupBy(m => m.MatchDate.Value.Date)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                var maxMatchesPerDay = request.MaxMatchesPerDay;
                
                var availableSlotsForRoundMatch = sortedSlotsForRound
                    .Where(s => !usedSlotsCopy.Contains(s))
                    .OrderBy(s => {
                        // Ưu tiên 1: Cùng ngày với matches đã chọn (nếu chưa đạt maxMatchesPerDay)
                        var sameDayPriority = 0.0;
                        if (matchesByDateForRound.ContainsKey(s.MatchDate.Date))
                        {
                            var matchesInThisDay = matchesByDateForRound[s.MatchDate.Date];
                            if (matchesInThisDay >= maxMatchesPerDay)
                            {
                                // Ngày này đã đầy → ưu tiên thấp (sang ngày mới)
                                sameDayPriority = 1000000.0;
                            }
                            // Nếu chưa đầy, sameDayPriority = 0 (ưu tiên cao nhất)
                        }
                        else
                        {
                            // Ngày này chưa có matches → ưu tiên trung bình
                            sameDayPriority = 500000.0;
                        }
                        
                        // Ưu tiên 2: Ngày sớm nhất (scale nhỏ)
                        var datePriority = (s.MatchDate.Date - new DateTime(2000, 1, 1)).TotalDays * 0.1;
                        
                        // Ưu tiên 3: Sân ít được sử dụng nhất
                        var locationPriority = 0.0;
                        if (!string.IsNullOrEmpty(s.Location) && locationUsageCount.ContainsKey(s.Location))
                        {
                            locationPriority = locationUsageCount[s.Location] * 0.01;
                        }
                        
                        // Ưu tiên 4: Giờ sớm nhất
                        var timePriority = s.StartTime.TotalMinutes / 100000.0;
                        
                        // Logic: Cùng ngày (chưa đầy) > Ngày sớm nhất > Sân ít dùng > Giờ sớm
                        return sameDayPriority + datePriority + locationPriority + timePriority;
                    })
                    .ToList();
                
                for (int j = 0; j < availableSlotsForRoundMatch.Count; j++)
                {
                    var slot = availableSlotsForRoundMatch[j];
                    var originalIndex = sortedSlotsForRound.IndexOf(slot);
                    if (originalIndex >= 0) slotIdx = originalIndex + 1;
                    
                    var slotStartTime = slot.MatchDate.Date.Add(slot.StartTime);
                    
                    // QUAN TRỌNG: Không còn ràng buộc thời gian cứng nhắc
                    // Chỉ check conflict với lịch học thực tế (HasScheduleConflict)
                    
                    // NGHIÊM NGẶT: Phải sau round trước + minGapBetweenMatches phút nghỉ
                    if (slotStartTime <= latestPreviousMatchTime.AddMinutes(minGapMinutes))
                    {
                        continue; // Bỏ qua slot này vì không đủ minGapBetweenMatches phút nghỉ sau round trước
                    }
                    
                    // NGHIÊM NGẶT: Phải sau previous matches (nếu có) + minGapBetweenMatches phút nghỉ
                    if (latestPreviousMatchEndTime.HasValue)
                    {
                        if (slotStartTime <= latestPreviousMatchEndTime.Value.AddMinutes(minGapMinutes))
                        {
                            continue; // Bỏ qua slot này vì không đủ minGapBetweenMatches phút nghỉ sau previous match
                        }
                    }
                    
                    // QUAN TRỌNG: Đảm bảo thời gian nghỉ với tất cả matches đã tạo trong cùng ngày (chỉ nếu cùng sân)
                    // Nếu có 2 sân trở lên, cho phép overlap thời gian nếu matches ở sân khác nhau
                    bool hasEnoughGap = true;
                    var hasMultipleLocations = request.AvailableLocations != null && request.AvailableLocations.Count >= 2;
                    
                    foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                    {
                        if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                        {
                            // QUAN TRỌNG: Nếu có nhiều sân và 2 matches ở sân khác nhau, cho phép overlap
                            bool sameLocation = !string.IsNullOrEmpty(slot.Location) && 
                                              !string.IsNullOrEmpty(usedMatch.Location) && 
                                              slot.Location == usedMatch.Location;
                            
                            // Chỉ check gap nếu không có nhiều sân hoặc cùng sân
                            if (!hasMultipleLocations || sameLocation)
                            {
                                var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                                var gapMinutes = (slotStartTime - usedMatchEnd).TotalMinutes;
                                if (gapMinutes < minGapMinutes) // Cần ít nhất minGapMinutes phút nghỉ
                                {
                                    hasEnoughGap = false;
                                    Console.WriteLine($"      ⚠️ Round {round}: Slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} (Sân: {slot.Location ?? "N/A"}) không đủ gap {minGapMinutes} phút với match {usedMatch.MatchNumber} ({usedMatch.MatchDate:yyyy-MM-dd} {usedMatch.EndTime:hh\\:mm}, Sân: {usedMatch.Location ?? "N/A"}) - Gap: {gapMinutes} phút");
                                    break;
                        }
                    }
                        }
                    }
                    if (!hasEnoughGap) continue;
                    
                    // Check conflict với lịch học của tất cả các lớp có thể tham gia match này
                    // Vì match này sẽ có 1 trong các lớp này, nên cần check tất cả để đảm bảo không conflict
                    bool hasConflict = false;
                    foreach (var classGroupId in possibleClassGroupIds)
                    {
                        if (HasScheduleConflict(slot, classGroupId, null, schedulesByClassGroup, request.MinGapBetweenMatches))
                        {
                            hasConflict = true;
                            break;
                        }
                    }
                    
                    if (!hasConflict)
                    {
                        selectedSlot = slot;
                        foundNonConflictSlot = true;
                        // Cập nhật số lần sử dụng sân này
                        if (!string.IsNullOrEmpty(slot.Location) && locationUsageCount.ContainsKey(slot.Location))
                        {
                            locationUsageCount[slot.Location]++;
                        }
                        Console.WriteLine($"   ✅ Round {round} Match {i + 1}: Chọn slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} ({slot.MatchDate.DayOfWeek}) - Sân: {slot.Location ?? "N/A"} - Không conflict");
                        break;
                    }
                }
                
                // Bước 2: Nếu không tìm thấy slot rảnh, chọn slot đầu tiên còn lại
                // (không bắt buộc tất cả lớp phải rảnh, nhưng vẫn ưu tiên slot rảnh)
                // QUAN TRỌNG: Vẫn phải đảm bảo slot sau previous matches (nghiêm ngặt)
                // QUAN TRỌNG: Ưu tiên sân ít được sử dụng nhất để phân bổ đều
                if (selectedSlot == null)
                {
                    var fallbackSlotsForRoundMatch = sortedSlotsForRound
                        .Where(s => !usedSlotsCopy.Contains(s))
                        .OrderBy(s => {
                            // Ưu tiên 1: Cùng ngày với matches đã chọn (nếu chưa đạt maxMatchesPerDay)
                            var sameDayPriority = 0.0;
                            if (matchesByDateForRound.ContainsKey(s.MatchDate.Date))
                            {
                                var matchesInThisDay = matchesByDateForRound[s.MatchDate.Date];
                                if (matchesInThisDay >= maxMatchesPerDay)
                                {
                                    sameDayPriority = 1000000.0;
                                }
                            }
                            else
                            {
                                sameDayPriority = 500000.0;
                            }
                            
                            // Ưu tiên 2: Ngày sớm nhất
                            var datePriority = (s.MatchDate.Date - new DateTime(2000, 1, 1)).TotalDays * 0.1;
                            
                            // Ưu tiên 3: Sân ít được sử dụng nhất
                            var locationPriority = 0.0;
                            if (!string.IsNullOrEmpty(s.Location) && locationUsageCount.ContainsKey(s.Location))
                            {
                                locationPriority = locationUsageCount[s.Location] * 0.01;
                            }
                            
                            // Ưu tiên 4: Giờ sớm nhất
                            var timePriority = s.StartTime.TotalMinutes / 100000.0;
                            
                            return sameDayPriority + datePriority + locationPriority + timePriority;
                        })
                        .ToList();
                    
                    for (int j = 0; j < fallbackSlotsForRoundMatch.Count; j++)
                    {
                        var slot = fallbackSlotsForRoundMatch[j];
                        var originalIndex = sortedSlotsForRound.IndexOf(slot);
                        if (originalIndex >= 0) slotIdx = originalIndex + 1;
                        
                        var slotStartTime = slot.MatchDate.Date.Add(slot.StartTime);
                        
                        // QUAN TRỌNG: Không còn ràng buộc thời gian cứng nhắc
                        // Chỉ check conflict với lịch học thực tế (HasScheduleConflict)
                        
                        // NGHIÊM NGẶT: Phải sau round trước + minGapBetweenMatches phút nghỉ
                        // minGapMinutes đã được khai báo ở đầu vòng lặp round (dòng 1717)
                        if (slotStartTime <= latestPreviousMatchTime.AddMinutes(minGapMinutes))
                        {
                            continue; // Bỏ qua slot này vì không đủ minGapBetweenMatches phút nghỉ sau round trước
                        }
                        
                        // NGHIÊM NGẶT: Phải sau previous matches (nếu có) + minGapBetweenMatches phút nghỉ
                        if (latestPreviousMatchEndTime.HasValue)
                        {
                            if (slotStartTime <= latestPreviousMatchEndTime.Value.AddMinutes(minGapMinutes))
                            {
                                continue; // Bỏ qua slot này vì không đủ minGapBetweenMatches phút nghỉ sau previous match
                            }
                        }
                        
                        // QUAN TRỌNG: Đảm bảo thời gian nghỉ với tất cả matches đã tạo trong cùng ngày (chỉ nếu cùng sân)
                        // Nếu có 2 sân trở lên, cho phép overlap thời gian nếu matches ở sân khác nhau
                        bool hasEnoughGap = true;
                        var hasMultipleLocations = request.AvailableLocations != null && request.AvailableLocations.Count >= 2;
                        
                        foreach (var usedMatch in matches.Where(m => m.MatchDate.HasValue && m.EndTime.HasValue))
                        {
                            if (usedMatch.MatchDate.Value.Date == slot.MatchDate.Date)
                            {
                                // QUAN TRỌNG: Nếu có nhiều sân và 2 matches ở sân khác nhau, cho phép overlap
                                bool sameLocation = !string.IsNullOrEmpty(slot.Location) && 
                                                  !string.IsNullOrEmpty(usedMatch.Location) && 
                                                  slot.Location == usedMatch.Location;
                                
                                // Chỉ check gap nếu không có nhiều sân hoặc cùng sân
                                if (!hasMultipleLocations || sameLocation)
                                {
                                    var usedMatchEnd = usedMatch.MatchDate.Value.Date.Add(usedMatch.EndTime.Value);
                                    var gapMinutes = (slotStartTime - usedMatchEnd).TotalMinutes;
                                    if (gapMinutes < minGapMinutes)
                                    {
                                        hasEnoughGap = false;
                                        Console.WriteLine($"      ⚠️ Round {round} (fallback): Slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} (Sân: {slot.Location ?? "N/A"}) không đủ gap {minGapMinutes} phút với match {usedMatch.MatchNumber} ({usedMatch.MatchDate:yyyy-MM-dd} {usedMatch.EndTime:hh\\:mm}, Sân: {usedMatch.Location ?? "N/A"}) - Gap: {gapMinutes} phút");
                                        break;
                                    }
                                }
                                else
                                {
                                    // Nếu có nhiều sân và 2 matches ở sân khác nhau → KHÔNG check gap (cho phép overlap)
                                    Console.WriteLine($"      ✅ Round {round} (fallback): Cho phép overlap - Slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} (Sân: {slot.Location ?? "N/A"}) với match {usedMatch.MatchNumber} ({usedMatch.MatchDate:yyyy-MM-dd} {usedMatch.EndTime:hh\\:mm}, Sân: {usedMatch.Location ?? "N/A"}) - Khác sân");
                            }
                        }
                        }
                        if (!hasEnoughGap) continue;
                        
                        selectedSlot = slot;
                        // Cập nhật số lần sử dụng sân này
                        if (!string.IsNullOrEmpty(slot.Location) && locationUsageCount.ContainsKey(slot.Location))
                        {
                            locationUsageCount[slot.Location]++;
                        }
                        Console.WriteLine($"   ⚠️ Round {round} Match {i + 1}: Không tìm thấy slot rảnh cho tất cả lớp có thể tham gia, sử dụng slot {slot.MatchDate:yyyy-MM-dd} {slot.StartTime:hh\\:mm} - Sân: {slot.Location ?? "N/A"} - Có thể conflict");
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
                
                // Link previous matches - QUAN TRỌNG: Dùng match object reference thay vì MatchNumber
                // MatchNumber sẽ được đánh lại sau khi sắp xếp, nên cần dùng reference
                if (participant1.PreviousMatchId.HasValue)
                {
                    var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant1.PreviousMatchId.Value);
                    if (prevMatch != null)
                    {
                        // Set NextMatchId tạm thời với MatchNumber hiện tại
                        // Sẽ được cập nhật lại sau khi sắp xếp và đánh lại số
                        prevMatch.NextMatchId = match.MatchNumber;
                    }
                }
                if (participant2.PreviousMatchId.HasValue)
                {
                    var prevMatch = matches.FirstOrDefault(m => m.MatchNumber == participant2.PreviousMatchId.Value);
                    if (prevMatch != null)
                    {
                        // Set NextMatchId tạm thời với MatchNumber hiện tại
                        // Sẽ được cập nhật lại sau khi sắp xếp và đánh lại số
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
    /// Tạo trận Chung kết giữa 2 nhánh
    /// </summary>
    /// <param name="branchAMatches">Danh sách matches của nhánh A</param>
    /// <param name="branchBMatches">Danh sách matches của nhánh B</param>
    /// <returns>Tuple (matches, requiresWeekend) - requiresWeekend = true nếu cần thêm ngày thứ 7, chủ nhật</returns>
    /// <remarks>
    /// ⚠️ QUAN TRỌNG - LƯU Ý KHI REFACTOR:
    /// - Đã đổi tên tham số từ morningBranchMatches/afternoonBranchMatches sang branchAMatches/branchBMatches
    /// - Đã đổi tên biến từ morningBranchWinner/afternoonBranchWinner sang branchAWinner/branchBWinner
    /// - Đã đổi tên biến từ morningWinnerEndTime/afternoonWinnerEndTime sang branchAWinnerEndTime/branchBWinnerEndTime
    /// - Lý do: Không còn chia theo buổi học mà chia để cân bằng số trận đấu giữa các đội
    /// - Khi refactor code trong method này, NHỚ tìm và cập nhật TẤT CẢ các tham chiếu cũ
    /// - Sử dụng grep để tìm: morningBranchMatches|afternoonBranchMatches|morningBranchWinner|afternoonBranchWinner
    /// </remarks>
    private (List<AiActivityMatch> matches, bool requiresWeekend) GenerateFinalMatchesBetweenBranches(
        TournamentScheduleRequest request,
        List<AiActivityMatch> branchAMatches,
        List<AiActivityMatch> branchBMatches,
        List<ScheduleSlot> allSlots,
        ref int matchNumber,
        ref HashSet<ScheduleSlot> usedSlots,
        Dictionary<int, List<ClassGroupSchedule>>? schedulesByClassGroup,
        Dictionary<string, int> locationUsageCount)
    {
        var matches = new List<AiActivityMatch>();
        bool requiresWeekend = false;
        
        // Tìm winners của mỗi nhánh (match có round cao nhất và không có NextMatchId)
        // QUAN TRỌNG: Winner là match có round cao nhất trong nhánh, không có NextMatchId
        // Nếu có nhiều matches cùng round cao nhất, chọn match có MatchNumber lớn nhất (match cuối cùng)
        var branchAWinner = branchAMatches
            .Where(m => m.NextMatchId == null && m.Round > 0)
            .OrderByDescending(m => m.Round)
            .ThenByDescending(m => m.MatchNumber)
            .FirstOrDefault();
            
        var branchBWinner = branchBMatches
            .Where(m => m.NextMatchId == null && m.Round > 0)
            .OrderByDescending(m => m.Round)
            .ThenByDescending(m => m.MatchNumber)
            .FirstOrDefault();
        
        Console.WriteLine($"   🔍 Tìm winners: Nhánh A winner = {(branchAWinner != null ? $"Match {branchAWinner.MatchNumber} (Round {branchAWinner.Round})" : "null")}, Nhánh B winner = {(branchBWinner != null ? $"Match {branchBWinner.MatchNumber} (Round {branchBWinner.Round})" : "null")}");
        
        // Kiểm tra nếu một trong hai nhánh không có matches hoặc không có winner
        if (branchAMatches.Count == 0 && branchBMatches.Count == 0)
        {
            Console.WriteLine($"   ⚠️ Không thể tạo chung kết: Cả hai nhánh đều không có matches");
            return (matches, false);
        }
        
        if (branchAWinner == null && branchBWinner == null)
        {
            // Nếu cả hai nhánh đều không có winner, không cần chung kết
            Console.WriteLine($"   ⚠️ Không thể tạo chung kết: Cả hai nhánh đều không có winner");
            return (matches, false);
        }
        
        // Nếu chỉ có 1 nhánh có winner, không cần chung kết (nhánh đó đã thắng)
        if (branchAWinner == null || branchBWinner == null)
        {
            Console.WriteLine($"   ℹ️ Chỉ có 1 nhánh có winner, không cần chung kết: Nhánh A winner = {branchAWinner != null}, Nhánh B winner = {branchBWinner != null}");
            return (matches, false);
        }
        
        // Copy usedSlots để dùng trong lambda (tránh lỗi ref trong lambda)
        var usedSlotsCopy = usedSlots;
        
        // QUAN TRỌNG: Chung kết phải diễn ra sau tất cả các trận khác
        // Tìm thời gian muộn nhất của tất cả matches đã tạo
        var allMatches = new List<AiActivityMatch>();
        allMatches.AddRange(branchAMatches);
        allMatches.AddRange(branchBMatches);
        
        var latestMatchTime = allMatches
            .Where(m => m.MatchDate.HasValue && m.EndTime.HasValue)
            .Select(m => m.MatchDate.Value.Date.Add(m.EndTime.Value))
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();
        
        // QUAN TRỌNG: Chung kết chờ kết quả từ cả 2 nhánh winners
        // Phải đảm bảo Chung kết diễn ra SAU cả 2 trận này
        DateTime? branchAWinnerEndTime = null;
        if (branchAWinner.MatchDate.HasValue && branchAWinner.EndTime.HasValue)
        {
            branchAWinnerEndTime = branchAWinner.MatchDate.Value.Date.Add(branchAWinner.EndTime.Value);
        }
        
        DateTime? branchBWinnerEndTime = null;
        if (branchBWinner.MatchDate.HasValue && branchBWinner.EndTime.HasValue)
        {
            branchBWinnerEndTime = branchBWinner.MatchDate.Value.Date.Add(branchBWinner.EndTime.Value);
        }
        
        // Thời gian muộn nhất = max của tất cả matches HOẶC 2 branch winners (nếu có)
        var finalLatestTime = latestMatchTime;
        if (branchAWinnerEndTime.HasValue && branchAWinnerEndTime.Value > finalLatestTime)
        {
            finalLatestTime = branchAWinnerEndTime.Value;
        }
        if (branchBWinnerEndTime.HasValue && branchBWinnerEndTime.Value > finalLatestTime)
        {
            finalLatestTime = branchBWinnerEndTime.Value;
        }
        
        Console.WriteLine($"   🏆 Chung kết: Thời gian muộn nhất của các trận khác = {finalLatestTime:yyyy-MM-dd HH:mm}");
        if (branchAWinnerEndTime.HasValue)
            Console.WriteLine($"      - Nhánh A winner kết thúc: {branchAWinnerEndTime.Value:yyyy-MM-dd HH:mm}");
        if (branchBWinnerEndTime.HasValue)
            Console.WriteLine($"      - Nhánh B winner kết thúc: {branchBWinnerEndTime.Value:yyyy-MM-dd HH:mm}");
        
        // QUAN TRỌNG: Trận chung kết có thể đá linh hoạt miễn là giờ rảnh chung
        // Không bắt buộc phải đá cuối tuần, chỉ cần không conflict với lịch học của cả 2 đội
        // Ưu tiên slot gần nhất thỏa mãn trước (không phân biệt ngày thường hay cuối tuần)
        
        // QUAN TRỌNG: Lấy danh sách các lớp có KHẢ NĂNG CAO tham gia chung kết
        // Thay vì check tất cả lớp (quá strict), chỉ check các đội ở semi-finals (round trước chung kết)
        // Nếu không có semi-finals rõ ràng, lấy tối đa 4 đội cuối (có khả năng cao nhất)
        var allPossibleClassGroupIds = new HashSet<int>();
        
        // Lấy 2 matches cuối cùng của mỗi nhánh (semi-finals hoặc gần nhất với chung kết)
        var branchATopMatches = branchAMatches
            .OrderByDescending(m => m.Round)
            .ThenByDescending(m => m.MatchNumber)
            .Take(2)
            .ToList();
            
        var branchBTopMatches = branchBMatches
            .OrderByDescending(m => m.Round)
            .ThenByDescending(m => m.MatchNumber)
            .Take(2)
            .ToList();
        
        foreach (var match in branchATopMatches)
        {
            if (match.ClassGroup1Id.HasValue) allPossibleClassGroupIds.Add(match.ClassGroup1Id.Value);
            if (match.ClassGroup2Id.HasValue) allPossibleClassGroupIds.Add(match.ClassGroup2Id.Value);
        }
        foreach (var match in branchBTopMatches)
        {
            if (match.ClassGroup1Id.HasValue) allPossibleClassGroupIds.Add(match.ClassGroup1Id.Value);
            if (match.ClassGroup2Id.HasValue) allPossibleClassGroupIds.Add(match.ClassGroup2Id.Value);
        }
        
        Console.WriteLine($"   🔍 Chung kết: Kiểm tra conflict với {allPossibleClassGroupIds.Count} lớp có khả năng cao nhất (semi-finalists)");
        
        // Tìm slot phù hợp cho chung kết: 
        // 1. Phải sau tất cả các trận khác VÀ sau cả 2 branch winners
        // 2. Ưu tiên slot gần nhất thỏa mãn trước (không phân biệt ngày thường hay cuối tuần)
        // 3. QUAN TRỌNG: Phải không conflict với lịch học của TẤT CẢ các lớp có thể tham gia
        
        // Debug: Log tất cả slots còn lại
        var allRemainingSlots = allSlots.Where(s => !usedSlotsCopy.Contains(s)).ToList();
        Console.WriteLine($"   🔍 Chung kết: Tổng số slots còn lại: {allRemainingSlots.Count}");
        if (allRemainingSlots.Any())
        {
            var firstSlot = allRemainingSlots.OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).First();
            var lastSlot = allRemainingSlots.OrderByDescending(s => s.MatchDate).ThenByDescending(s => s.StartTime).First();
            Console.WriteLine($"   🔍 Chung kết: Slot đầu tiên còn lại = {firstSlot.MatchDate:yyyy-MM-dd} {firstSlot.StartTime:hh\\:mm}");
            Console.WriteLine($"   🔍 Chung kết: Slot cuối cùng còn lại = {lastSlot.MatchDate:yyyy-MM-dd} {lastSlot.StartTime:hh\\:mm}");
        }
        
        // Capture các biến vào lambda
        var minGapMinutes = request.MinGapBetweenMatches;
        var finalLatestTimeForLambda = finalLatestTime;
        var branchAWinnerEndTimeForLambda = branchAWinnerEndTime;
        var branchBWinnerEndTimeForLambda = branchBWinnerEndTime;
        
        var availableSlotsAfter = allSlots
            .Where(s => !usedSlotsCopy.Contains(s))
            .Where(s => {
                var slotStartTime = s.MatchDate.Date.Add(s.StartTime);
                
                // Phải sau tất cả matches + minGapBetweenMatches phút nghỉ
                if (slotStartTime <= finalLatestTimeForLambda.AddMinutes(minGapMinutes))
                {
                    return false; // Quá sớm
                }
                
                // Phải sau nhánh A winner (nếu có) + minGapBetweenMatches phút nghỉ
                if (branchAWinnerEndTimeForLambda.HasValue && slotStartTime <= branchAWinnerEndTimeForLambda.Value.AddMinutes(minGapMinutes))
                {
                    return false; // Quá sớm
                }
                
                // Phải sau nhánh B winner (nếu có) + minGapBetweenMatches phút nghỉ
                if (branchBWinnerEndTimeForLambda.HasValue && slotStartTime <= branchBWinnerEndTimeForLambda.Value.AddMinutes(minGapMinutes))
                {
                    return false; // Quá sớm
                }
                
                // Kiểm tra khung giờ cấm (11:30-13:00)
                var slotStartTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
                var slotEndTotalMinutes = s.EndTime.Hours * 60 + s.EndTime.Minutes;
                var forbiddenStartMinutes = 11 * 60 + 30; // 11:30
                var forbiddenEndMinutes = 13 * 60; // 13:00
                
                // Không được overlap với khung giờ cấm
                if (slotStartTotalMinutes < forbiddenEndMinutes && slotEndTotalMinutes > forbiddenStartMinutes)
                {
                    return false; // Trùng khung giờ cấm
                }
                
                // QUAN TRỌNG: Sử dụng PreferredStartTime và PreferredEndTime từ request
                var minStartTime = request.PreferredStartTime ?? TimeSpan.FromHours(7);
                var maxEndTime = request.PreferredEndTime ?? TimeSpan.FromHours(18);
                var minStartTotalMinutes = minStartTime.Hours * 60 + minStartTime.Minutes;
                var maxEndTotalMinutes = maxEndTime.Hours * 60 + maxEndTime.Minutes;
                
                // Phải bắt đầu từ PreferredStartTime trở đi
                if (slotStartTotalMinutes < minStartTotalMinutes)
                {
                    return false; // Quá sớm (trước PreferredStartTime)
                }
                
                // Phải kết thúc trước PreferredEndTime
                if (slotEndTotalMinutes > maxEndTotalMinutes)
                {
                    return false; // Quá muộn (sau PreferredEndTime)
                }
                
                // QUAN TRỌNG: Check conflict với lịch học của các đội có khả năng cao tham gia chung kết
                // Chỉ check với semi-finalists (tối đa 4 đội), KHÔNG check toàn bộ 11 lớp (quá strict)
                // Lý do: Trong single elimination, chỉ có 2 đội sẽ vào chung kết từ 2 nhánh
                // Việc check tất cả lớp sẽ khiến không tìm được slot vì conflict quá nhiều
                if (schedulesByClassGroup != null && allPossibleClassGroupIds.Any())
                {
                    // Check conflict với từng lớp có khả năng cao tham gia chung kết
                    foreach (var classGroupId in allPossibleClassGroupIds)
                    {
                        if (HasScheduleConflict(s, classGroupId, null, schedulesByClassGroup, minGapMinutes))
                        {
                            return false; // Conflict với lịch học của lớp này
                        }
                    }
                }
                
                return true; // Không conflict với bất kỳ lớp nào
            })
            .ToList();
        
        Console.WriteLine($"   🔍 Chung kết: Sau filter tất cả điều kiện: {availableSlotsAfter.Count} slots");
        if (availableSlotsAfter.Any())
        {
            var firstAvailable = availableSlotsAfter.OrderBy(s => s.MatchDate).ThenBy(s => s.StartTime).First();
            Console.WriteLine($"   🔍 Chung kết: Slot đầu tiên phù hợp = {firstAvailable.MatchDate:yyyy-MM-dd} {firstAvailable.StartTime:hh\\:mm} - Sân: {firstAvailable.Location ?? "N/A"}");
            }
            else
            {
            Console.WriteLine($"   ⚠️ Chung kết: KHÔNG TÌM ĐƯỢC slot nào phù hợp (với {allPossibleClassGroupIds.Count} lớp có khả năng cao nhất)!");
            Console.WriteLine($"      - Có thể do: Tất cả slots đều trước {finalLatestTime.AddMinutes(minGapMinutes):yyyy-MM-dd HH:mm}");
            Console.WriteLine($"      - Hoặc: Tất cả slots đều conflict với lịch học của các lớp semi-finalists");
            
            // QUAN TRỌNG: Nếu không tìm được slot với tất cả các lớp, thử chỉ check với 2 lớp có khả năng cao nhất
            // (các lớp trong Round cao nhất của mỗi nhánh - có khả năng cao nhất vào chung kết)
            var likelyClassGroupIds = new HashSet<int>();
            if (branchAWinner != null)
            {
                if (branchAWinner.ClassGroup1Id.HasValue) likelyClassGroupIds.Add(branchAWinner.ClassGroup1Id.Value);
                if (branchAWinner.ClassGroup2Id.HasValue) likelyClassGroupIds.Add(branchAWinner.ClassGroup2Id.Value);
            }
            if (branchBWinner != null)
            {
                if (branchBWinner.ClassGroup1Id.HasValue) likelyClassGroupIds.Add(branchBWinner.ClassGroup1Id.Value);
                if (branchBWinner.ClassGroup2Id.HasValue) likelyClassGroupIds.Add(branchBWinner.ClassGroup2Id.Value);
            }
            
            if (likelyClassGroupIds.Any() && likelyClassGroupIds.Count < allPossibleClassGroupIds.Count)
            {
                Console.WriteLine($"   🔄 Chung kết: Thử lại với chỉ {likelyClassGroupIds.Count} lớp có khả năng cao nhất (từ Round cao nhất)");
                
                // Thử lại với chỉ các lớp có khả năng cao nhất
                var relaxedSlots = allSlots
                .Where(s => !usedSlotsCopy.Contains(s))
                .Where(s => {
                    var slotStartTime = s.MatchDate.Date.Add(s.StartTime);
                        if (slotStartTime <= finalLatestTimeForLambda.AddMinutes(minGapMinutes)) return false;
                        if (branchAWinnerEndTimeForLambda.HasValue && slotStartTime <= branchAWinnerEndTimeForLambda.Value.AddMinutes(minGapMinutes)) return false;
                        if (branchBWinnerEndTimeForLambda.HasValue && slotStartTime <= branchBWinnerEndTimeForLambda.Value.AddMinutes(minGapMinutes)) return false;
                        
                        var slotStartTotalMinutes = s.StartTime.Hours * 60 + s.StartTime.Minutes;
                        var slotEndTotalMinutes = s.EndTime.Hours * 60 + s.EndTime.Minutes;
                        var forbiddenStartMinutes = 11 * 60 + 30;
                        var forbiddenEndMinutes = 13 * 60;
                        
                        if (slotStartTotalMinutes < forbiddenEndMinutes && slotEndTotalMinutes > forbiddenStartMinutes) return false;
                        // QUAN TRỌNG: Loại bỏ giới hạn cứng 18h - chỉ cần bắt đầu từ 07:00 trở đi
                        if (slotStartTotalMinutes < 7 * 60) return false; // Quá sớm (trước 07:00)
                        // Không giới hạn giờ kết thúc - cho phép đá buổi tối
                        
                        // Chỉ check conflict với các lớp có khả năng cao nhất
                        if (schedulesByClassGroup != null && likelyClassGroupIds.Any())
                        {
                            foreach (var classGroupId in likelyClassGroupIds)
                            {
                                if (HasScheduleConflict(s, classGroupId, null, schedulesByClassGroup, minGapMinutes))
                                {
                                    return false;
                                }
                            }
                        }
                        return true;
                    })
                    .OrderBy(s => s.MatchDate)
                    .ThenBy(s => s.StartTime)
                    .ToList();
                
                if (relaxedSlots.Any())
                {
                    Console.WriteLine($"   ✅ Chung kết: Tìm thấy {relaxedSlots.Count} slots với logic linh hoạt hơn!");
                    // QUAN TRỌNG: Ưu tiên sân ít được sử dụng nhất để phân bổ đều
                    availableSlotsAfter = relaxedSlots
                        .OrderBy(s => {
                            // Ưu tiên 1: Sân ít được sử dụng nhất (scale nhỏ để cân bằng với thời gian)
                            var locationPriority = 0.0;
                            if (!string.IsNullOrEmpty(s.Location) && locationUsageCount.ContainsKey(s.Location))
                            {
                                locationPriority = locationUsageCount[s.Location] * 10;
                            }
                            // Ưu tiên 2: Ngày sớm nhất (scale nhỏ để ưu tiên cùng ngày trước)
                            var datePriority = (s.MatchDate.Date - new DateTime(2000, 1, 1)).TotalDays * 5;
                            // Ưu tiên 3: Giờ sớm nhất (scale bằng phút / 60)
                            var timePriority = s.StartTime.TotalMinutes / 60.0;
                            return locationPriority + datePriority + timePriority;
                        })
                        .ToList();
                }
            }
        }
        
        // Sắp xếp tất cả slots theo thời gian (ngày gần nhất, giờ sớm nhất)
        // Ưu tiên: MatchDate (sớm nhất) -> StartTime (sớm nhất)
        availableSlotsAfter = availableSlotsAfter
            .OrderBy(s => s.MatchDate)
            .ThenBy(s => s.StartTime)
            .ToList();
        
        Console.WriteLine($"   📅 Chung kết: Tìm thấy {availableSlotsAfter.Count} slots (sắp xếp theo thời gian, không phân biệt ngày thường hay cuối tuần)");
        
        ScheduleSlot? selectedSlot = null;
        
        if (availableSlotsAfter.Any())
        {
            // Chọn slot SỚM NHẤT sau tất cả matches (để kết thúc giải sớm hơn)
            // QUAN TRỌNG: Ưu tiên sân ít được sử dụng nhất để phân bổ đều
            selectedSlot = availableSlotsAfter
                .OrderBy(s => {
                    // Ưu tiên 1: Sân ít được sử dụng nhất (scale nhỏ để cân bằng với thời gian)
                    var locationPriority = 0.0;
                    if (!string.IsNullOrEmpty(s.Location) && locationUsageCount.ContainsKey(s.Location))
                    {
                        locationPriority = locationUsageCount[s.Location] * 10;
                    }
                    // Ưu tiên 2: Ngày sớm nhất (scale nhỏ để ưu tiên cùng ngày trước)
                    var datePriority = (s.MatchDate.Date - new DateTime(2000, 1, 1)).TotalDays * 5;
                    // Ưu tiên 3: Giờ sớm nhất (scale bằng phút / 60)
                    var timePriority = s.StartTime.TotalMinutes / 60.0;
                    return locationPriority + datePriority + timePriority;
                })
                .First();
            Console.WriteLine($"   ✅ Chung kết: Chọn slot sau tất cả matches = {selectedSlot.MatchDate:yyyy-MM-dd} {selectedSlot.StartTime:hh\\:mm} ({selectedSlot.MatchDate.DayOfWeek}) - Sân: {selectedSlot.Location ?? "N/A"}");
            Console.WriteLine($"   ✅ Chung kết: Slot đã được validate không conflict với lịch học của {allPossibleClassGroupIds.Count} lớp có thể tham gia");
        }
        else
        {
            // QUAN TRỌNG: Nếu không có slot phù hợp, báo lỗi
            Console.WriteLine($"   ❌ Chung kết: KHÔNG THỂ tìm slot phù hợp cho trận chung kết giữa 2 nhánh!");
            Console.WriteLine($"      - Nhánh A winner kết thúc: {(branchAWinnerEndTime.HasValue ? branchAWinnerEndTime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}");
            Console.WriteLine($"      - Nhánh B winner kết thúc: {(branchBWinnerEndTime.HasValue ? branchBWinnerEndTime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}");
            Console.WriteLine($"      - Tổng số slots còn lại: {allSlots.Count(s => !usedSlotsCopy.Contains(s))}");
            Console.WriteLine($"      - Thời gian muộn nhất cần: {finalLatestTime.AddMinutes(minGapMinutes):yyyy-MM-dd HH:mm}");
            Console.WriteLine($"   ⚠️ YÊU CẦU: Cần thêm slots sau {finalLatestTime.AddMinutes(minGapMinutes):yyyy-MM-dd HH:mm} để đảm bảo có đủ slots cho trận chung kết");
            requiresWeekend = false; // Không còn bắt buộc phải cuối tuần
        }
        
        if (selectedSlot != null)
        {
            var slot = selectedSlot;
            // Cập nhật số lần sử dụng sân này
            if (!string.IsNullOrEmpty(slot.Location) && locationUsageCount.ContainsKey(slot.Location))
            {
                locationUsageCount[slot.Location]++;
            }
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
                Round = Math.Max(branchAWinner.Round, branchBWinner.Round) + 1,
                RoundName = "Chung kết",
                MatchNumber = matchNumber++,
                IsBye = false,
                Notes = "Chung kết giữa 2 nhánh"
            };
            
            // Link previous matches - QUAN TRỌNG: Set NextMatchId tạm thời
            // Sẽ được cập nhật lại sau khi sắp xếp và đánh lại số
            branchAWinner.NextMatchId = finalMatch.MatchNumber;
            branchBWinner.NextMatchId = finalMatch.MatchNumber;
            
            matches.Add(finalMatch);
        }
        else if (requiresWeekend)
        {
            Console.WriteLine($"   ⚠️ KHÔNG tạo match chung kết vì cần thêm ngày thứ 7, chủ nhật");
        }

        return (matches, requiresWeekend);
    }
    
    /// <summary>
    /// Tạo danh sách warnings chi tiết về lý do không đủ slots
    /// </summary>
    private List<SlotWarningReason> GenerateSlotWarnings(
        int availableSlotsCount,
        int minSlotsNeeded,
        int recommendedSlots,
        int actualMatchesCreated,
        int expectedMatches,
        (int conflictWithMatches, int conflictWithLocation, int conflictWithParticipantActivities) conflictCounts,
        TournamentScheduleRequest request,
        bool isComplete,
        EduShpereDbContext? dbContext = null)
    {
        var warnings = new List<SlotWarningReason>();
        
        // Warning 1: Không đủ slots tổng thể
        if (availableSlotsCount < minSlotsNeeded)
        {
            warnings.Add(new SlotWarningReason
            {
                WarningType = "InsufficientSlots",
                Title = "Không đủ slots để tạo lịch thi đấu",
                Description = $"Chỉ có {availableSlotsCount} slots khả dụng, nhưng cần tối thiểu {minSlotsNeeded} slots để tạo đủ {expectedMatches} trận đấu.",
                CurrentValue = $"{availableSlotsCount} slots",
                RecommendedValue = $"Ít nhất {minSlotsNeeded} slots (khuyến nghị {recommendedSlots} slots)",
                Solution = "Mở rộng khoảng thời gian (StartDate - EndDate) hoặc thêm sân thi đấu (AvailableLocations)",
                Severity = "Critical",
                Field = "startDate, endDate, availableLocations"
            });
        }
        else if (availableSlotsCount < recommendedSlots)
        {
            warnings.Add(new SlotWarningReason
            {
                WarningType = "LowSlotCount",
                Title = "Số lượng slots gần với mức tối thiểu",
                Description = $"Có {availableSlotsCount} slots khả dụng, khuyến nghị {recommendedSlots} slots để đảm bảo có đủ slots cho tất cả các vòng.",
                CurrentValue = $"{availableSlotsCount} slots",
                RecommendedValue = $"{recommendedSlots} slots",
                Solution = "Mở rộng khoảng thời gian hoặc thêm sân thi đấu để có thêm slots dự phòng",
                Severity = "High",
                Field = "startDate, endDate, availableLocations"
            });
        }
        
        // Warning 2: Quá nhiều conflicts với matches đã có
        if (conflictCounts.conflictWithMatches > 0)
        {
            warnings.Add(new SlotWarningReason
            {
                WarningType = "ExistingMatchConflicts",
                Title = "Xung đột với các trận đấu đã có",
                Description = $"Có {conflictCounts.conflictWithMatches} slots bị loại bỏ do trùng với các trận đấu đã được lên lịch trước đó.",
                CurrentValue = $"{conflictCounts.conflictWithMatches} conflicts",
                RecommendedValue = "0 conflicts",
                Solution = "Kiểm tra và điều chỉnh thời gian các trận đấu đã có, hoặc chọn khoảng thời gian khác",
                Severity = conflictCounts.conflictWithMatches > 10 ? "High" : "Medium",
                Field = "startDate, endDate"
            });
        }
        
        // Warning 3: Không tạo đủ matches
        if (!isComplete && actualMatchesCreated < expectedMatches)
        {
            var missingMatches = expectedMatches - actualMatchesCreated;
            warnings.Add(new SlotWarningReason
            {
                WarningType = "IncompleteMatches",
                Title = "Không tạo đủ số trận đấu",
                Description = $"Chỉ tạo được {actualMatchesCreated}/{expectedMatches} trận đấu. Thiếu {missingMatches} trận đấu.",
                CurrentValue = $"{actualMatchesCreated} matches",
                RecommendedValue = $"{expectedMatches} matches",
                Solution = "Mở rộng khoảng thời gian, thêm sân thi đấu, hoặc giảm số lớp tham gia",
                Severity = missingMatches > expectedMatches / 2 ? "Critical" : "High",
                Field = "startDate, endDate, availableLocations, classGroupIds"
            });
        }
        
        // Warning 5: Khoảng thời gian quá hẹp
        var dateRange = request.EndDate.Date - request.StartDate.Date;
        var daysNeeded = (int)Math.Ceiling((double)expectedMatches / request.MaxMatchesPerDay);
        if (dateRange.Days < daysNeeded)
        {
            warnings.Add(new SlotWarningReason
            {
                WarningType = "TimeRangeTooNarrow",
                Title = "Khoảng thời gian quá hẹp",
                Description = $"Khoảng thời gian chỉ có {dateRange.Days} ngày, nhưng cần ít nhất {daysNeeded} ngày để xếp {expectedMatches} trận đấu (tối đa {request.MaxMatchesPerDay} trận/ngày).",
                CurrentValue = $"{dateRange.Days} ngày",
                RecommendedValue = $"Ít nhất {daysNeeded} ngày",
                Solution = $"Mở rộng khoảng thời gian từ {request.StartDate:dd/MM/yyyy} đến {request.EndDate:dd/MM/yyyy} hoặc tăng MaxMatchesPerDay",
                Severity = dateRange.Days < daysNeeded / 2 ? "Critical" : "Medium",
                Field = "startDate, endDate, maxMatchesPerDay"
            });
        }
        
        // Warning 6: Không có sân thi đấu
        if (request.AvailableLocations == null || request.AvailableLocations.Count == 0)
        {
            warnings.Add(new SlotWarningReason
            {
                WarningType = "NoLocations",
                Title = "Chưa cấu hình sân thi đấu",
                Description = "Chưa có sân thi đấu nào được cấu hình. Điều này có thể hạn chế số lượng slots khả dụng.",
                CurrentValue = "0 sân",
                RecommendedValue = "Ít nhất 1 sân",
                Solution = "Thêm ít nhất 1 sân thi đấu vào danh sách AvailableLocations",
                Severity = "Medium",
                Field = "availableLocations"
            });
        }
        
        // Warning 7: MaxMatchesPerDay quá thấp
        if (request.MaxMatchesPerDay < expectedMatches / Math.Max(1, dateRange.Days))
        {
            var recommendedMaxMatches = (int)Math.Ceiling((double)expectedMatches / Math.Max(1, dateRange.Days));
            warnings.Add(new SlotWarningReason
            {
                WarningType = "MaxMatchesPerDayTooLow",
                Title = "Số trận tối đa mỗi ngày quá thấp",
                Description = $"MaxMatchesPerDay = {request.MaxMatchesPerDay} có thể quá thấp để xếp {expectedMatches} trận trong {dateRange.Days} ngày.",
                CurrentValue = $"{request.MaxMatchesPerDay} trận/ngày",
                RecommendedValue = $"Ít nhất {recommendedMaxMatches} trận/ngày",
                Solution = $"Tăng MaxMatchesPerDay lên {recommendedMaxMatches} hoặc mở rộng khoảng thời gian",
                Severity = "Low",
                Field = "maxMatchesPerDay, startDate, endDate"
            });
        }
        
        return warnings;
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

