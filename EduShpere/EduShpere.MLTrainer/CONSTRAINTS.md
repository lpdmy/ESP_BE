# 📋 Danh Sách Ràng Buộc Đã Implement

## ✅ Các Ràng Buộc Hiện Có

### 1. **Ràng Buộc Cơ Bản**
- ✅ **Không chọn slot không available**: Slot đã bị đánh dấu không available sẽ không được chọn
- ✅ **Không conflict thời gian**: Hai slot overlap về thời gian không thể được chọn cùng lúc
- ✅ **Tối đa số activity mỗi ngày**: Giới hạn số activity có thể tham gia trong một ngày (mặc định: 3)

### 2. **Ràng Buộc Về Hoạt Động**
- ✅ **Không conflict với activities đã có**: Không chọn slot trùng với activity đã đăng ký
- ✅ **Max Participants (Capacity)**: Không chọn slot khi activity đã đầy người
- ✅ **Deadline đăng ký**: Không chọn slot sau deadline đăng ký

### 3. **Ràng Buộc Về Thời Gian**
- ✅ **Khoảng cách tối thiểu giữa các activity**: Đảm bảo có thời gian nghỉ giữa các hoạt động (mặc định: 30 phút)
- ✅ **Thời gian làm việc (Working Hours)**: Chỉ chọn slot trong giờ làm việc (mặc định: 8h-20h)

### 4. **Ràng Buộc Về Địa Điểm**
- ✅ **Location conflict**: Hai activity cùng địa điểm và overlap thời gian không thể được chọn
- ✅ **Excluded locations**: Có thể loại trừ một số địa điểm không được phép

## 🎯 Objective Function

Hệ thống tối đa hóa:
- **ML Score tổng thể**: Chọn các slot có điểm AI dự đoán cao nhất
- **Tránh conflicts**: Tự động loại bỏ các slot conflict

## 📝 Cấu Trúc ScheduleConstraint

```csharp
public class ScheduleConstraint
{
    public int UserId { get; set; }
    public List<int> ConflictingActivityIds { get; set; } // Activities đã đăng ký
    public TimeSpan? MinGapBetweenActivities { get; set; } // Khoảng cách tối thiểu (ví dụ: 30 phút)
    public TimeSpan? WorkingHoursStart { get; set; } // Giờ làm việc bắt đầu (ví dụ: 8h)
    public TimeSpan? WorkingHoursEnd { get; set; } // Giờ làm việc kết thúc (ví dụ: 20h)
    public List<string>? ExcludedLocations { get; set; } // Địa điểm không được chọn
    public Dictionary<int, int>? ActivityMaxParticipants { get; set; } // Max participants cho mỗi activity
    public Dictionary<int, DateTime>? ActivityRegisterDeadline { get; set; } // Deadline đăng ký
}
```

## 📝 Cấu Trúc TimeSlot

```csharp
public class TimeSlot
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int ActivityId { get; set; }
    public float MLScore { get; set; } // Điểm từ ML model (0-1)
    public bool IsAvailable { get; set; } // Slot có available không
    public string? Location { get; set; } // Địa điểm
    public int CurrentParticipants { get; set; } // Số người đã đăng ký
    public int MaxParticipants { get; set; } // Số người tối đa
}
```

## 🔧 Cách Sử Dụng

### Tạo Constraints Cơ Bản
```csharp
var constraints = new List<ORToolsScheduler.ScheduleConstraint>
{
    new ORToolsScheduler.ScheduleConstraint
    {
        UserId = userId,
        ConflictingActivityIds = existingActivityIds,
        MinGapBetweenActivities = TimeSpan.FromMinutes(30),
        WorkingHoursStart = TimeSpan.FromHours(8),
        WorkingHoursEnd = TimeSpan.FromHours(20)
    }
};
```

### Tạo Constraints Nâng Cao
```csharp
var constraints = new List<ORToolsScheduler.ScheduleConstraint>
{
    new ORToolsScheduler.ScheduleConstraint
    {
        UserId = userId,
        ConflictingActivityIds = existingActivityIds,
        MinGapBetweenActivities = TimeSpan.FromMinutes(60), // 1 giờ nghỉ
        WorkingHoursStart = TimeSpan.FromHours(7), // Từ 7h sáng
        WorkingHoursEnd = TimeSpan.FromHours(22), // Đến 10h tối
        ExcludedLocations = new List<string> { "Phòng A", "Phòng B" },
        ActivityMaxParticipants = new Dictionary<int, int>
        {
            { activityId1, 50 },
            { activityId2, 100 }
        },
        ActivityRegisterDeadline = new Dictionary<int, DateTime>
        {
            { activityId1, DateTime.Now.AddDays(7) }
        }
    }
};
```

## 🎓 Ví Dụ Ràng Buộc

### Ví dụ 1: Tránh Conflict Thời Gian
```
Activity A: 9h-11h
Activity B: 10h-12h
→ Không thể chọn cả 2 (overlap 10h-11h)
```

### Ví dụ 2: Khoảng Cách Tối Thiểu
```
Activity A: 9h-11h
Activity B: 11h-13h
→ Nếu MinGap = 30 phút: Không thể chọn cả 2 (chỉ cách 0 phút)
→ Nếu MinGap = 0: Có thể chọn cả 2
```

### Ví dụ 3: Location Conflict
```
Activity A: 9h-11h tại "Phòng A"
Activity B: 9h-11h tại "Phòng A"
→ Không thể chọn cả 2 (cùng địa điểm, cùng thời gian)
```

### Ví dụ 4: Working Hours
```
Slot: 7h-9h
WorkingHours: 8h-20h
→ Không được chọn (bắt đầu trước 8h)
```

## 📊 Thứ Tự Áp Dụng Ràng Buộc

1. **Ràng buộc 1**: Loại bỏ slot không available
2. **Ràng buộc 2**: Kiểm tra conflict thời gian
3. **Ràng buộc 3**: Giới hạn số activity mỗi ngày
4. **Ràng buộc 4**: Loại bỏ activities đã đăng ký
5. **Ràng buộc 5**: Kiểm tra khoảng cách tối thiểu
6. **Ràng buộc 6**: Kiểm tra working hours
7. **Ràng buộc 7**: Kiểm tra location conflict
8. **Ràng buộc 8**: Kiểm tra deadline đăng ký
9. **Ràng buộc 9**: Kiểm tra max participants

## 🚀 Tối Ưu Hóa

Hệ thống sử dụng **OR-Tools CP-SAT** để:
- Tìm giải pháp tối ưu (optimal solution)
- Hoặc giải pháp khả thi (feasible solution)
- Tối đa hóa tổng ML score
- Đảm bảo tất cả ràng buộc được thỏa mãn

## 💡 Tips

1. **MinGapBetweenActivities**: Điều chỉnh theo nhu cầu (30 phút, 1 giờ, 2 giờ...)
2. **WorkingHours**: Có thể khác nhau cho từng user
3. **MaxActivitiesPerDay**: Có thể điều chỉnh (mặc định: 3)
4. **Location conflict**: Chỉ áp dụng khi có thông tin Location

---

**Lưu ý**: Tất cả các ràng buộc đều được áp dụng tự động khi gọi `OptimizeSchedule()`.

