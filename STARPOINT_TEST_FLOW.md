# Flow Test StarPoint System

## Tổng quan
Hệ thống StarPoint có 2 luồng chính:
1. **Luồng Tham gia (Participation Points)**: Tự động cộng điểm cho người tham gia đã hoàn thành hoạt động
2. **Luồng Giải thưởng (Rank Rewards)**: Trao điểm thưởng dựa trên thứ hạng giải thưởng

---

## 1. Luồng Tham gia (Participation Points)

### Mục đích
Tự động cộng điểm cho tất cả participants đã tham gia hoạt động sau khi hoạt động kết thúc.

### Điều kiện tiên quyết
1. Activity đã được tạo với `ActivityRegistrationReward` (có điểm thưởng đăng ký)
2. Activity đã kết thúc (`EndDate < Now`)
3. Có participants với `Status = Joined` và không bị xóa

### Flow Test

#### Bước 1: Chuẩn bị dữ liệu
```sql
-- 1. Tạo Activity với EndDate trong quá khứ
INSERT INTO Activities (Title, Description, StartDate, EndDate, RegisterDate, EndRegisterDate, ...)
VALUES ('Test Activity', 'Test', '2024-01-01', '2024-01-10', '2024-01-01', '2024-01-05', ...);

-- 2. Tạo ActivityRegistrationReward với điểm thưởng
INSERT INTO ActivityRegistrationRewards (ActivityId, StarPoints)
VALUES (1, 10); -- 10 điểm khi tham gia

-- 3. Tạo Participants với Status = Joined
INSERT INTO ActivityParticipants (ActivityId, UserId, Status, IsDeleted)
VALUES 
    (1, 1, 0, 0), -- Joined = 0
    (1, 2, 0, 0),
    (1, 3, 0, 0);
```

#### Bước 2: Gọi API
```http
POST /api/activity/{activityId}/award-participation-points
Authorization: Bearer {admin_token}
```

#### Bước 3: Kiểm tra kết quả

**Expected Response:**
```json
{
  "statusCode": 200,
  "message": "Đã cộng điểm tham gia cho 3 người tham gia thành công",
  "data": 3
}
```

**Kiểm tra Database:**
```sql
-- Kiểm tra UserPoint.Balance đã được cập nhật
SELECT UserId, Balance FROM UserPoint WHERE UserId IN (1, 2, 3);
-- Expected: Balance = 10 cho mỗi user

-- Kiểm tra PointHistory đã được tạo
SELECT UserId, Points, Description, ActionType 
FROM PointHistory 
WHERE UserId IN (1, 2, 3) 
ORDER BY CreatedAt DESC;
-- Expected: 3 records với Points = 10, ActionType = 0 (Earn), Description = "Điểm tham gia hoạt động: Test Activity"
```

#### Test Cases

**TC1: Activity chưa kết thúc**
- Input: Activity với `EndDate = 2025-12-31` (tương lai)
- Expected: Error 400 - "Activity has not ended yet. Cannot award participation points."

**TC2: Activity không có RegistrationReward**
- Input: Activity không có `ActivityRegistrationReward`
- Expected: Response với `data = 0` (không có ai được cộng điểm)

**TC3: Không có participants**
- Input: Activity không có participants với `Status = Joined`
- Expected: Response với `data = 0`

**TC4: Transaction rollback khi có lỗi**
- Input: User không tồn tại (UserId = 999)
- Expected: Transaction rollback, không có điểm nào được cộng

---

## 2. Luồng Giải thưởng (Rank Rewards)

### Mục đích
Trao điểm thưởng cho participants dựa trên thứ hạng giải thưởng (Giải Nhất, Giải Nhì, Giải Ba, ...).

### Điều kiện tiên quyết
1. Activity đã có `ActivityReward` config (rank và điểm tương ứng)
2. Participants đã tồn tại và không bị xóa
3. Người gọi API có quyền Admin/Staff

### Flow Test

#### Bước 1: Chuẩn bị dữ liệu
```sql
-- 1. Tạo Activity
INSERT INTO Activities (Title, Description, ...)
VALUES ('Cuộc thi Vẽ', 'Cuộc thi vẽ tranh', ...);

-- 2. Tạo ActivityReward config
INSERT INTO ActivityRewards (ActivityId, Rank, StarPoints)
VALUES 
    (1, 'Giải Nhất', 100),
    (1, 'Giải Nhì', 50),
    (1, 'Giải Ba', 30),
    (1, 'Khuyến khích', 10);

-- 3. Tạo Participants
INSERT INTO ActivityParticipants (ActivityId, UserId, Status, IsDeleted)
VALUES 
    (1, 1, 0, 0), -- Participant ID = 1
    (1, 2, 0, 0), -- Participant ID = 2
    (1, 3, 0, 0), -- Participant ID = 3
    (1, 4, 0, 0); -- Participant ID = 4
```

#### Bước 2: Gọi API
```http
POST /api/activity/{activityId}/award-rank-rewards
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "participantRanks": {
    "1": "Giải Nhất",    // Participant ID 1 -> Giải Nhất (100 điểm)
    "2": "Giải Nhì",     // Participant ID 2 -> Giải Nhì (50 điểm)
    "3": "Giải Ba",      // Participant ID 3 -> Giải Ba (30 điểm)
    "4": "Khuyến khích"  // Participant ID 4 -> Khuyến khích (10 điểm)
  }
}
```

#### Bước 3: Kiểm tra kết quả

**Expected Response:**
```json
{
  "statusCode": 200,
  "message": "Đã trao giải thưởng thành công",
  "data": true
}
```

**Kiểm tra Database:**
```sql
-- Kiểm tra UserPoint.Balance đã được cập nhật
SELECT UserId, Balance FROM UserPoint WHERE UserId IN (1, 2, 3, 4);
-- Expected: 
-- UserId 1: Balance = 100 (Giải Nhất)
-- UserId 2: Balance = 50 (Giải Nhì)
-- UserId 3: Balance = 30 (Giải Ba)
-- UserId 4: Balance = 10 (Khuyến khích)

-- Kiểm tra PointHistory đã được tạo
SELECT UserId, Points, Description, ActionType 
FROM PointHistory 
WHERE UserId IN (1, 2, 3, 4) 
ORDER BY CreatedAt DESC;
-- Expected: 4 records với:
-- UserId 1: Points = 100, Description = "Giải thưởng Giải Nhất - Hoạt động: Cuộc thi Vẽ"
-- UserId 2: Points = 50, Description = "Giải thưởng Giải Nhì - Hoạt động: Cuộc thi Vẽ"
-- UserId 3: Points = 30, Description = "Giải thưởng Giải Ba - Hoạt động: Cuộc thi Vẽ"
-- UserId 4: Points = 10, Description = "Giải thưởng Khuyến khích - Hoạt động: Cuộc thi Vẽ"
```

#### Test Cases

**TC1: Rank không tồn tại trong config**
- Input: `"participantRanks": { "1": "Giải Đặc Biệt" }` (không có trong ActivityReward)
- Expected: Error 400 - "Rank 'Giải Đặc Biệt' not found in reward configuration"

**TC2: Participant không tồn tại**
- Input: `"participantRanks": { "999": "Giải Nhất" }` (Participant ID 999 không tồn tại)
- Expected: Error 400 - "Participant 999 not found or deleted"

**TC3: Participant ID không hợp lệ**
- Input: `"participantRanks": { "abc": "Giải Nhất" }` (không phải số)
- Expected: Error 400 - "Participant ID không hợp lệ: abc"

**TC4: Request body rỗng**
- Input: `{ "participantRanks": {} }`
- Expected: Error 400 - "Participant ranks không được để trống"

**TC5: Activity không có Reward config**
- Input: Activity không có `ActivityReward`
- Expected: Error 400 - "No reward configuration found for activity {id}"

**TC6: Transaction rollback khi có lỗi**
- Input: Một participant hợp lệ và một participant không tồn tại
- Expected: Transaction rollback, không có điểm nào được cộng

---

## 3. Test Transaction Integrity

### Mục đích
Đảm bảo tính toàn vẹn dữ liệu: nếu có lỗi, cả `UserPoint.Balance` và `PointHistory` đều không được cập nhật.

### Test Case

**TC: Simulate lỗi trong transaction**
```csharp
// Trong PointHistoryService.AddPointsWithTransactionAsync
// Thêm code để simulate lỗi:
throw new Exception("Simulated error");
```

**Expected:**
- `UserPoint.Balance` không được cập nhật
- `PointHistory` không được tạo
- Transaction được rollback

**Kiểm tra:**
```sql
-- Trước khi gọi API
SELECT UserId, Balance FROM UserPoint WHERE UserId = 1;
-- Giả sử Balance = 50

-- Sau khi gọi API (có lỗi)
SELECT UserId, Balance FROM UserPoint WHERE UserId = 1;
-- Expected: Balance vẫn = 50 (không thay đổi)

SELECT COUNT(*) FROM PointHistory WHERE UserId = 1 AND Description LIKE '%Test%';
-- Expected: 0 (không có record mới)
```

---

## 4. Test Concurrency

### Mục đích
Đảm bảo khi nhiều request cùng lúc cộng điểm cho cùng một user, dữ liệu vẫn nhất quán.

### Test Case

**TC: Concurrent requests**
```http
# Request 1 (Thread 1)
POST /api/activity/1/award-participation-points

# Request 2 (Thread 2) - cùng lúc
POST /api/activity/1/award-participation-points
```

**Expected:**
- Cả 2 requests đều thành công
- `UserPoint.Balance` được cập nhật đúng (tăng 2 lần điểm thưởng)
- Có 2 `PointHistory` records được tạo

**Kiểm tra:**
```sql
-- Giả sử điểm thưởng = 10
SELECT UserId, Balance FROM UserPoint WHERE UserId = 1;
-- Expected: Balance = 20 (10 + 10)

SELECT COUNT(*) FROM PointHistory WHERE UserId = 1 AND Description LIKE '%Test Activity%';
-- Expected: 2 records
```

---

## 5. Test Edge Cases

### TC1: User chưa có UserPoint record
- Input: User mới chưa có `UserPoint` record
- Expected: Tự động tạo `UserPoint` với `Balance = 0`, sau đó cộng điểm

### TC2: Points = 0 hoặc âm
- Input: `points = 0` hoặc `points < 0`
- Expected: Error - "Points must be greater than 0"

### TC3: Redeem với balance không đủ
- Input: `actionType = Redeem`, `points = 100`, nhưng `Balance = 50`
- Expected: Error - "Insufficient balance. Current: 50, Required: 100"

### TC4: Adjustment action type
- Input: `actionType = Adjustment`, `points = 200`
- Expected: `Balance` được set trực tiếp = 200 (không cộng/trừ)

---

## 6. Checklist Test

### Luồng Tham gia
- [ ] Activity chưa kết thúc → Error
- [ ] Activity không có RegistrationReward → Return 0
- [ ] Không có participants → Return 0
- [ ] Có participants → Cộng điểm thành công
- [ ] UserPoint.Balance được cập nhật đúng
- [ ] PointHistory được tạo đúng
- [ ] Transaction rollback khi có lỗi

### Luồng Giải thưởng
- [ ] Rank không tồn tại → Error
- [ ] Participant không tồn tại → Error
- [ ] Participant ID không hợp lệ → Error
- [ ] Request body rỗng → Error
- [ ] Activity không có Reward config → Error
- [ ] Trao giải thành công → Return true
- [ ] UserPoint.Balance được cập nhật đúng theo rank
- [ ] PointHistory được tạo đúng
- [ ] Transaction rollback khi có lỗi

### Transaction Integrity
- [ ] Lỗi trong transaction → Rollback
- [ ] UserPoint và PointHistory không được cập nhật khi rollback

### Concurrency
- [ ] Concurrent requests → Dữ liệu nhất quán

### Edge Cases
- [ ] User chưa có UserPoint → Tự động tạo
- [ ] Points = 0 hoặc âm → Error
- [ ] Redeem với balance không đủ → Error
- [ ] Adjustment action type → Set balance trực tiếp

---

## 7. Postman Collection

### Request 1: Award Participation Points
```http
POST http://localhost:7084/api/activity/1/award-participation-points
Authorization: Bearer {admin_token}
```

### Request 2: Award Rank Rewards
```http
POST http://localhost:7084/api/activity/1/award-rank-rewards
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "participantRanks": {
    "1": "Giải Nhất",
    "2": "Giải Nhì",
    "3": "Giải Ba"
  }
}
```

---

## 8. SQL Scripts để Test

### Setup Test Data
```sql
-- 1. Tạo Activity đã kết thúc
INSERT INTO Activities (Title, Description, StartDate, EndDate, RegisterDate, EndRegisterDate, Category, SubType, ThumbnailUrl, Organizer, CreatedAt, IsDeleted, RowVersion)
VALUES ('Test Activity', 'Test Description', '2024-01-01', '2024-01-10', '2024-01-01', '2024-01-05', 1, 'SportsFestival', 'test.jpg', 'Test Organizer', GETUTCDATE(), 0, 0x00);

-- 2. Tạo RegistrationReward
INSERT INTO ActivityRegistrationRewards (ActivityId, StarPoints, CreatedAt, UpdatedAt, IsDeleted, RowVersion)
VALUES (1, 10, GETUTCDATE(), GETUTCDATE(), 0, 0x00);

-- 3. Tạo Rank Rewards
INSERT INTO ActivityRewards (ActivityId, Rank, StarPoints, CreatedAt, UpdatedAt, IsDeleted, RowVersion)
VALUES 
    (1, 'Giải Nhất', 100, GETUTCDATE(), GETUTCDATE(), 0, 0x00),
    (1, 'Giải Nhì', 50, GETUTCDATE(), GETUTCDATE(), 0, 0x00),
    (1, 'Giải Ba', 30, GETUTCDATE(), GETUTCDATE(), 0, 0x00);

-- 4. Tạo Participants
INSERT INTO ActivityParticipants (ActivityId, UserId, Status, IsDeleted, CreatedAt, RowVersion)
VALUES 
    (1, 1, 0, 0, GETUTCDATE(), 0x00), -- Joined
    (1, 2, 0, 0, GETUTCDATE(), 0x00),
    (1, 3, 0, 0, GETUTCDATE(), 0x00);
```

### Verify Results
```sql
-- Kiểm tra UserPoint
SELECT UserId, Balance, CreatedAt, UpdatedAt 
FROM UserPoint 
WHERE UserId IN (1, 2, 3);

-- Kiểm tra PointHistory
SELECT UserId, Points, Description, ActionType, CreatedAt 
FROM PointHistory 
WHERE UserId IN (1, 2, 3) 
ORDER BY CreatedAt DESC;
```

---

## 9. Notes

- Tất cả các API đều yêu cầu quyền **Admin** hoặc **Staff**
- Transaction đảm bảo tính toàn vẹn: nếu có lỗi, cả `UserPoint` và `PointHistory` đều không được cập nhật
- `UserPoint` được tự động tạo nếu user chưa có record
- `ActionType` mặc định là `Earn` (0), có thể là `Redeem` (1) hoặc `Adjustment` (2)



