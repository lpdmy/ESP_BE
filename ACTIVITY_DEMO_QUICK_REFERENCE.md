# ⚡ ACTIVITY DEMO - QUICK REFERENCE

## 🎯 CÁC BƯỚC DEMO NHANH

### 1. TẠO ACTIVITY (Teacher/Admin)

```bash
POST /api/activity
Authorization: Bearer {token}

# CreativeContest
{
  "title": "Cuộc thi vẽ tranh mùa xuân",
  "description": "...",
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-03-31T23:59:59Z",
  "location": "Sân trường",
  "category": 1,
  "subType": "CreativeContest",
  "thumbnailUrl": "https://...",
  "organizer": "Ban giám hiệu",
  "registerDate": "2024-02-01T00:00:00Z",
  "endRegisterDate": "2024-02-28T23:59:59Z",
  "maxParticipants": 100,
  "rules": ["Rule 1", "Rule 2"],
  "theme": "Mùa xuân quê hương",
  "genre": "Vẽ tranh",
  "paperSize": "A3",
  "drawingMedium": "Màu nước",
  "starPointRewards": {
    "registration": "50",
    "awards": [
      {"name": "Giải nhất", "points": "500"},
      {"name": "Giải nhì", "points": "300"}
    ]
  },
  "gradingSettings": {
    "criteria": [
      "Tính sáng tạo (40 điểm)",
      "Kỹ thuật vẽ (30 điểm)",
      "Bố cục và màu sắc (30 điểm)"
    ]
  },
  "onlyTeacherCanRegister": false
}
```

### 2. XEM DANH SÁCH ACTIVITY

```bash
GET /api/activity?pageNumber=1&pageSize=10&search=thi
Authorization: Bearer {token}
```

### 3. XEM CHI TIẾT ACTIVITY

```bash
GET /api/activity/{id}
Authorization: Bearer {token}
```

### 4. ĐĂNG KÝ THAM GIA (Student)

```bash
POST /api/activityparticipant
Authorization: Bearer {token}

{
  "activityId": 1,
  "userId": 123  // Lấy từ token
}
```

**Kết quả**: Tự động cộng StarPoint nếu có RegistrationReward

### 5. NỘP BÀI (Student)

```bash
POST /api/submission
Authorization: Bearer {token}

{
  "activityId": 1,
  "title": "Tác phẩm của tôi",
  "attachments": [
    {
      "url": "https://cloudinary.com/file.jpg",
      "fileName": "tranh.jpg",
      "fileType": "image/jpeg"
    }
  ]
}
```

### 6. XEM SUBMISSION

```bash
# Tất cả submission của activity
GET /api/submission/activity/{id}?pageNumber=1&pageSize=10

# Submission của mình
GET /api/submission/my-submissions?pageNumber=1&pageSize=10

# Submission của mình trong activity cụ thể
GET /api/submission/activity/{activityId}/my-submission
```

### 7. CẬP NHẬT SUBMISSION

```bash
PUT /api/submission/{id}
Authorization: Bearer {token}

{
  "id": 1,
  "title": "Tác phẩm đã chỉnh sửa",
  "attachments": [...]
}
```

### 8. XÓA SUBMISSION

```bash
DELETE /api/submission/{id}
Authorization: Bearer {token}
```

---

## 📋 CÁC LOẠI ACTIVITY

### CreativeContest
- **Fields**: theme, genre, paperSize, drawingMedium, timeLimit, submissionFormat
- **Use case**: Cuộc thi vẽ tranh, sáng tác văn học, nhiếp ảnh...

### SportsFestival
- **Fields**: sportsCategories (list), competitionType (Individual/Team/Mixed)
- **Use case**: Hội thao, giải thể thao

### SeminarWorkshop
- **Fields**: speakers (list), programItems (list)
- **Use case**: Hội thảo, workshop, buổi chia sẻ

---

## 🔑 KEY FEATURES

### Grading
- Set `gradingSettings.criteria` → `IsGrade = true`
- Submission có thể có `Score`

### StarPoint Rewards
- `registration`: Điểm khi đăng ký
- `awards`: Điểm khi đạt giải

### Registration Control
- `onlyTeacherCanRegister`: Chỉ teacher đăng ký được
- Kiểm tra thời gian: RegisterDate <= now <= EndRegisterDate
- Kiểm tra số lượng: currentParticipants < MaxParticipants

---

## ⚠️ VALIDATION RULES

1. **Thời gian**:
   - StartDate < EndDate
   - RegisterDate < EndRegisterDate
   - EndRegisterDate < StartDate

2. **Số lượng**:
   - MaxParticipants > 0

3. **Unique**:
   - (ActivityId, UserId) trong ActivityParticipant
   - Một user chỉ có 1 Submission cho mỗi Activity

4. **Quyền**:
   - Student: Chỉ tạo/cập nhật/xóa Submission của mình
   - Teacher/Admin: Tạo Activity, xem tất cả

---

## 🎬 DEMO SCENARIOS

### Scenario 1: CreativeContest với Grading
1. Teacher tạo Activity (CreativeContest) với grading
2. Student đăng ký → Nhận StarPoint
3. Student nộp bài với attachments
4. Teacher xem danh sách submission
5. Teacher chấm điểm (update Score)

### Scenario 2: SportsFestival
1. Teacher tạo Activity (SportsFestival)
2. Student đăng ký
3. Student nộp kết quả (nếu cần)

### Scenario 3: SeminarWorkshop
1. Teacher tạo Activity (SeminarWorkshop) với speakers & program
2. Student/Teacher đăng ký
3. Không cần nộp bài

---

## 📊 RESPONSE FORMAT

### Success
```json
{
  "data": {...},
  "message": "Thành công",
  "statusCode": 200
}
```

### Error
```json
{
  "data": null,
  "message": "Lỗi...",
  "statusCode": 400
}
```

---

## 🔐 AUTHENTICATION

Tất cả API cần:
```
Authorization: Bearer {jwt_token}
```

Lấy token từ:
```
POST /api/auth/login
{
  "username": "...",
  "password": "..."
}
```

---

**Xem chi tiết trong ACTIVITY_DEMO_FLOW.md**


