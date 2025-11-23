using Google.OrTools.Sat;
using EduShpere.Domain.Models;

namespace EduShpere.MLTrainer.Services;

/// <summary>
/// Service sử dụng OR-Tools để tối ưu hóa lịch, tránh conflict
/// </summary>
public class ORToolsScheduler
{
    /// <summary>
    /// Slot thời gian có thể đặt
    /// </summary>
    public class TimeSlot
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ActivityId { get; set; }
        public float MLScore { get; set; } // Điểm từ ML model
        public bool IsAvailable { get; set; } = true;
        public string? Location { get; set; } // Địa điểm (để check location conflict)
        public int CurrentParticipants { get; set; } // Số người đã đăng ký
        public int MaxParticipants { get; set; } // Số người tối đa
    }

    /// <summary>
    /// Ràng buộc cho scheduling
    /// </summary>
    public class ScheduleConstraint
    {
        public int UserId { get; set; }
        public List<int> ConflictingActivityIds { get; set; } = new();
        public TimeSpan? MinDuration { get; set; }
        public TimeSpan? MaxDuration { get; set; }
        public List<TimeSpan> PreferredTimeSlots { get; set; } = new();
        
        // Ràng buộc mới
        public TimeSpan? MinGapBetweenActivities { get; set; } // Khoảng cách tối thiểu giữa các activity
        public TimeSpan? WorkingHoursStart { get; set; } // Giờ làm việc bắt đầu (ví dụ: 8h)
        public TimeSpan? WorkingHoursEnd { get; set; } // Giờ làm việc kết thúc (ví dụ: 20h)
        public List<string>? ExcludedLocations { get; set; } // Địa điểm không được chọn
        public Dictionary<int, int>? ActivityMaxParticipants { get; set; } // Max participants cho mỗi activity
        public Dictionary<int, DateTime>? ActivityRegisterDeadline { get; set; } // Deadline đăng ký
    }

    /// <summary>
    /// Kết quả tối ưu hóa
    /// </summary>
    public class ScheduleResult
    {
        public bool IsOptimal { get; set; }
        public List<TimeSlot> SelectedSlots { get; set; } = new();
        public string? Explanation { get; set; }
        public double ObjectiveValue { get; set; }
    }

    /// <summary>
    /// Tối ưu hóa lịch sử dụng OR-Tools CP-SAT
    /// </summary>
    public ScheduleResult OptimizeSchedule(
        List<TimeSlot> availableSlots,
        List<ScheduleConstraint> constraints,
        int maxActivitiesPerDay = 3)
    {
        var model = new CpModel();

        // Biến quyết định: chọn slot nào (0 = không chọn, 1 = chọn)
        var slotVars = new Dictionary<int, IntVar>();
        foreach (var slot in availableSlots)
        {
            slotVars[slot.Id] = model.NewBoolVar($"slot_{slot.Id}");
        }

        // Objective: Tối đa hóa tổng ML score và minimize conflicts
        var objectiveTerms = new List<LinearExpr>();
        foreach (var slot in availableSlots)
        {
            if (slot.IsAvailable)
            {
                // Thêm điểm ML score (nhân 1000 để làm tròn)
                objectiveTerms.Add((int)(slot.MLScore * 1000) * slotVars[slot.Id]);
            }
        }
        model.Maximize(LinearExpr.Sum(objectiveTerms));

        // Ràng buộc 1: Không chọn slot không available
        foreach (var slot in availableSlots.Where(s => !s.IsAvailable))
        {
            model.Add(slotVars[slot.Id] == 0);
        }

        // Ràng buộc 2: Không conflict thời gian
        for (int i = 0; i < availableSlots.Count; i++)
        {
            for (int j = i + 1; j < availableSlots.Count; j++)
            {
                var slot1 = availableSlots[i];
                var slot2 = availableSlots[j];

                // Nếu 2 slot overlap về thời gian
                if (slot1.StartTime < slot2.EndTime && slot2.StartTime < slot1.EndTime)
                {
                    // Không thể chọn cả 2
                    model.Add(slotVars[slot1.Id] + slotVars[slot2.Id] <= 1);
                }
            }
        }

        // Ràng buộc 3: Tối đa số activity mỗi ngày
        var slotsByDate = availableSlots.GroupBy(s => s.StartTime.Date);
        foreach (var dayGroup in slotsByDate)
        {
            var daySlots = dayGroup.ToList();
            var daySlotVars = daySlots.Select(s => slotVars[s.Id]).ToList();
            model.Add(LinearExpr.Sum(daySlotVars) <= maxActivitiesPerDay);
        }

        // Ràng buộc 4: Không conflict với activities đã có
        foreach (var constraint in constraints)
        {
            foreach (var conflictingId in constraint.ConflictingActivityIds)
            {
                var conflictingSlots = availableSlots.Where(s => s.ActivityId == conflictingId).ToList();
                foreach (var slot in conflictingSlots)
                {
                    if (slotVars.ContainsKey(slot.Id))
                    {
                        model.Add(slotVars[slot.Id] == 0);
                    }
                }
            }
        }

        // Ràng buộc 5: Khoảng cách tối thiểu giữa các activity
        foreach (var constraint in constraints)
        {
            if (constraint.MinGapBetweenActivities.HasValue)
            {
                var minGapMinutes = (int)constraint.MinGapBetweenActivities.Value.TotalMinutes;
                
                for (int i = 0; i < availableSlots.Count; i++)
                {
                    for (int j = i + 1; j < availableSlots.Count; j++)
                    {
                        var slot1 = availableSlots[i];
                        var slot2 = availableSlots[j];
                        
                        // Nếu 2 slot quá gần nhau (nhỏ hơn minGap)
                        var gap = Math.Abs((slot1.StartTime - slot2.StartTime).TotalMinutes);
                        if (gap > 0 && gap < minGapMinutes)
                        {
                            // Không thể chọn cả 2 nếu quá gần
                            model.Add(slotVars[slot1.Id] + slotVars[slot2.Id] <= 1);
                        }
                    }
                }
            }
        }

        // Ràng buộc 6: Thời gian làm việc (working hours)
        foreach (var constraint in constraints)
        {
            if (constraint.WorkingHoursStart.HasValue && constraint.WorkingHoursEnd.HasValue)
            {
                foreach (var slot in availableSlots)
                {
                    var slotTime = slot.StartTime.TimeOfDay;
                    // Nếu slot ngoài giờ làm việc
                    if (slotTime < constraint.WorkingHoursStart.Value || 
                        slotTime > constraint.WorkingHoursEnd.Value)
                    {
                        if (slotVars.ContainsKey(slot.Id))
                        {
                            model.Add(slotVars[slot.Id] == 0);
                        }
                    }
                }
            }
        }

        // Ràng buộc 7: Địa điểm conflict (nếu 2 activity cùng địa điểm và overlap thời gian)
        for (int i = 0; i < availableSlots.Count; i++)
        {
            for (int j = i + 1; j < availableSlots.Count; j++)
            {
                var slot1 = availableSlots[i];
                var slot2 = availableSlots[j];
                
                // Nếu cùng địa điểm và overlap thời gian
                if (!string.IsNullOrEmpty(slot1.Location) && 
                    !string.IsNullOrEmpty(slot2.Location) &&
                    slot1.Location == slot2.Location &&
                    slot1.StartTime < slot2.EndTime && 
                    slot2.StartTime < slot1.EndTime)
                {
                    // Không thể chọn cả 2
                    model.Add(slotVars[slot1.Id] + slotVars[slot2.Id] <= 1);
                }
            }
        }

        // Ràng buộc 7b: Loại trừ địa điểm không được phép
        foreach (var constraint in constraints)
        {
            if (constraint.ExcludedLocations != null && constraint.ExcludedLocations.Any())
            {
                foreach (var slot in availableSlots)
                {
                    if (!string.IsNullOrEmpty(slot.Location) && 
                        constraint.ExcludedLocations.Contains(slot.Location))
                    {
                        if (slotVars.ContainsKey(slot.Id))
                        {
                            model.Add(slotVars[slot.Id] == 0);
                        }
                    }
                }
            }
        }

        // Ràng buộc 8: Max participants (capacity constraint)
        foreach (var constraint in constraints)
        {
            if (constraint.ActivityMaxParticipants != null)
            {
                foreach (var kvp in constraint.ActivityMaxParticipants)
                {
                    var activityId = kvp.Key;
                    var maxParticipants = kvp.Value;
                    
                    var activitySlots = availableSlots
                        .Where(s => s.ActivityId == activityId && s.CurrentParticipants >= maxParticipants)
                        .ToList();
                    
                    // Không chọn slot đã đầy
                    foreach (var slot in activitySlots)
                    {
                        if (slotVars.ContainsKey(slot.Id))
                        {
                            model.Add(slotVars[slot.Id] == 0);
                        }
                    }
                }
            }
        }

        // Ràng buộc 8: Deadline đăng ký
        foreach (var constraint in constraints)
        {
            if (constraint.ActivityRegisterDeadline != null)
            {
                foreach (var kvp in constraint.ActivityRegisterDeadline)
                {
                    var activityId = kvp.Key;
                    var deadline = kvp.Value;
                    
                    // Không chọn slot sau deadline
                    var lateSlots = availableSlots
                        .Where(s => s.ActivityId == activityId && s.StartTime > deadline)
                        .ToList();
                    
                    foreach (var slot in lateSlots)
                    {
                        if (slotVars.ContainsKey(slot.Id))
                        {
                            model.Add(slotVars[slot.Id] == 0);
                        }
                    }
                }
            }
        }

        // Ràng buộc 9: Tối thiểu số activity được chọn (nếu cần)
        // Có thể thêm: model.Add(LinearExpr.Sum(slotVars.Values) >= minActivities);

        // Solve
        var solver = new CpSolver();
        var status = solver.Solve(model);

        var result = new ScheduleResult
        {
            IsOptimal = status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible
        };

        if (result.IsOptimal)
        {
            result.ObjectiveValue = solver.ObjectiveValue;
            result.SelectedSlots = availableSlots
                .Where(slot => slotVars.ContainsKey(slot.Id) && solver.Value(slotVars[slot.Id]) == 1)
                .OrderBy(s => s.StartTime)
                .ToList();

            // Tạo explanation
            result.Explanation = GenerateExplanation(result.SelectedSlots, availableSlots);
        }
        else
        {
            result.Explanation = "Không tìm được lịch phù hợp với các ràng buộc hiện tại.";
        }

        return result;
    }

    /// <summary>
    /// Tạo giải thích tại sao chọn các slot này
    /// </summary>
    private string GenerateExplanation(List<TimeSlot> selectedSlots, List<TimeSlot> allSlots)
    {
        if (selectedSlots.Count == 0)
        {
            return "Không có slot nào được chọn.";
        }

        var explanations = new List<string>();

        foreach (var slot in selectedSlots)
        {
            var mlScorePercent = (int)(slot.MLScore * 100);
            var timeStr = slot.StartTime.ToString("HH:mm");
            var dayStr = slot.StartTime.ToString("dddd", new System.Globalization.CultureInfo("vi-VN"));

            explanations.Add(
                $"Slot {timeStr} ngày {dayStr} được chọn vì:" +
                $" AI dự đoán {mlScorePercent}% phù hợp với thói quen của bạn" +
                $" và không trùng với các hoạt động khác."
            );
        }

        return string.Join("\n", explanations);
    }

    /// <summary>
    /// Tìm slot tốt nhất cho một activity cụ thể
    /// </summary>
    public TimeSlot? FindBestSlotForActivity(
        int activityId,
        List<TimeSlot> availableSlots,
        List<ScheduleConstraint> constraints)
    {
        // Filter slots cho activity này
        var activitySlots = availableSlots
            .Where(s => s.ActivityId == activityId && s.IsAvailable)
            .ToList();

        if (activitySlots.Count == 0)
        {
            return null;
        }

        // Sắp xếp theo ML score
        var bestSlot = activitySlots
            .OrderByDescending(s => s.MLScore)
            .FirstOrDefault();

        // Kiểm tra conflict
        foreach (var constraint in constraints)
        {
            if (constraint.ConflictingActivityIds.Contains(activityId))
            {
                // Tìm slot không conflict
                var nonConflictingSlots = activitySlots
                    .Where(s => !constraint.ConflictingActivityIds.Contains(s.ActivityId))
                    .ToList();

                if (nonConflictingSlots.Count > 0)
                {
                    bestSlot = nonConflictingSlots.OrderByDescending(s => s.MLScore).First();
                }
            }
        }

        return bestSlot;
    }
}

