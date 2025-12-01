# 🎯 LUỒNG DEMO PHẦN ACTIVITY

## 📋 TỔNG QUAN HỆ THỐNG ACTIVITY

### 1. Cấu trúc Activity

**Activity** là hệ thống quản lý hoạt động trong EduSphere, bao gồm:

- **ActivityType**: 
  - `Activity = 1`: Hoạt động thường
  - `Event = 2`: Sự kiện

- **SubType** (phân loại chi tiết):
  - **SportsFestival**: Lễ hội thể thao
  - **CreativeContest**: Cuộc thi sáng tạo
  - **SeminarWorkshop**: Hội thảo/Workshop

### 2. Các thành phần chính

#### 2.1. Activity Entity
- Thông tin cơ bản: Title, Description, StartDate, EndDate, Location
- Đăng ký: RegisterDate, EndRegisterDate, MaxParticipants
- Cấu hình: Category, SubType, ThumbnailUrl, Organizer
- **Grading**: IsGrade (bool), GradingSettings (JSON - danh sách tiêu chí chấm điểm)
- **Registration**: OnlyTeacherCanRegister (bool) - chỉ giáo viên mới đăng ký được

#### 2.2. ActivityDetail
- **SportsFestival**: CompetitionType (Individual/Team/Mixed)
- **CreativeContest**: Theme, Genre, PaperSize, DrawingMedium, TimeLimit, SubmissionFormat

#### 2.3. ActivityParticipant
- Quản lý người tham gia
- Status: Joined, Pending, Rejected
- Unique constraint: (ActivityId, UserId)

#### 2.4. Submission
- Bài nộp của học sinh cho Activity
- Có thể có nhiều Attachments (file đính kèm)
- Có thể có Score (điểm số) nếu Activity có grading

#### 2.5. StarPoint Rewards
- **RegistrationReward**: Điểm thưởng khi đăng ký
- **Awards**: Danh sách giải thưởng với điểm thưởng tương ứng

---

## 🔄 LUỒNG HOẠT ĐỘNG CHI TIẾT

### PHASE 1: TẠO ACTIVITY (Teacher/Admin)

#### 1.1. Tạo Activity cơ bản

**API**: `POST /api/activity`

**Request Body**:
```json
{
  "title": "Cuộc thi vẽ tranh mùa xuân 2024",
  "description": "Cuộc thi vẽ tranh với chủ đề mùa xuân...",
  "startDate": "2024-03-01T00:00:00Z",
  "endDate": "2024-03-31T23:59:59Z",
  "location": "Sân trường",
  "category": 1,  // Activity = 1
  "subType": "CreativeContest",
  "thumbnailUrl": "https://cloudinary.com/image.jpg",
  "organizer": "Ban giám hiệu",
  "registerDate": "2024-02-01T00:00:00Z",
  "endRegisterDate": "2024-02-28T23:59:59Z",
  "maxParticipants": 100,
  "clubId": null,  // Optional
  
  // Rules
  "rules": [
    "Bài dự thi phải là tác phẩm gốc",
    "Không được sao chép từ nguồn khác"
  ],
  
  // CreativeContest specific
  "theme": "Mùa xuân quê hương",
  "genre": "Vẽ tranh",
  "paperSize": "A3",
  "drawingMedium": "Màu nước",
  "timeLimit": "Tự do",
  "submissionFormat": "File số (JPG, PNG)",
  
  // StarPoint Rewards
  "starPointRewards": {
    "registration": "50",  // Điểm khi đăng ký
    "awards": [
      { "name": "Giải nhất", "points": "500" },
      { "name": "Giải nhì", "points": "300" },
      { "name": "Giải ba", "points": "200" }
    ]
  },
  
  // Grading Settings (nếu có chấm điểm)
  "gradingSettings": {
    "criteria": [
      "Tính sáng tạo (40 điểm)",
      "Kỹ thuật vẽ (30 điểm)",
      "Bố cục và màu sắc (30 điểm)"
    ]
  },
  
  // Registration Settings
  "onlyTeacherCanRegister": false  // false = học sinh có thể đăng ký
}
```

**Response**: ActivityResponseDto với đầy đủ thông tin

#### 1.2. Tạo SportsFestival Activity

```json
{
  "title": "Hội thao mùa xuân 2024",
  "subType": "SportsFestival",
  "sportsCategories": [
    "Chạy 100m",
    "Chạy 400m",
    "Nhảy cao",
    "Bóng đá"
  ],
  "competitionType": "Mixed",  // Individual, Team, Mixed
  // ... các field khác
}
```

#### 1.3. Tạo SeminarWorkshop Activity

```json
{
  "title": "Workshop lập trình Python",
  "subType": "SeminarWorkshop",
  "speakers": [
    {
      "name": "Nguyễn Văn A",
      "title": "Senior Developer",
      "bio": "10 năm kinh nghiệm...",
      "imageUrl": "https://..."
    }
  ],
  "programItems": [
    {
      "title": "Giới thiệu Python",
      "time": "9:00 - 9:30",
      "description": "..."
    },
    {
      "title": "Thực hành",
      "time": "9:30 - 11:00",
      "description": "..."
    }
  ],
  // ... các field khác
}
```

#### 1.4. Cập nhật Activity

**API**: `PUT /api/activity`

Tương tự CreateActivityDto nhưng tất cả fields đều optional (trừ Id)

---

### PHASE 2: XEM DANH SÁCH ACTIVITY

#### 2.1. Lấy danh sách Activity (có phân trang)

**API**: `GET /api/activity?pageNumber=1&pageSize=10&search=thi`

**Response**:
```json
{
  "data": {
    "data": [
      {
        "id": 1,
        "title": "Cuộc thi vẽ tranh...",
        "description": "...",
        "startDate": "2024-03-01T00:00:00Z",
        "endDate": "2024-03-31T23:59:59Z",
        "location": "Sân trường",
        "category": 1,
        "subType": "CreativeContest",
        "thumbnailUrl": "...",
        "numberOfParticipants": 45,
        "maxParticipants": 100,
        "registerDate": "2024-02-01T00:00:00Z",
        "endRegisterDate": "2024-02-28T23:59:59Z",
        "rules": [...],
        "activityDetail": {
          "theme": "Mùa xuân quê hương",
          "genre": "Vẽ tranh",
          ...
        },
        "registrationReward": {
          "starPoints": 50
        },
        "awards": [...],
        "gradingSettings": {
          "criteria": [...]
        },
        "onlyTeacherCanRegister": false
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10
  },
  "message": "Lấy danh sách thành công",
  "statusCode": 200
}
```

#### 2.2. Lấy chi tiết Activity theo ID

**API**: `GET /api/activity/{id}`

---

### PHASE 3: ĐĂNG KÝ THAM GIA (Student/Teacher)

#### 3.1. Đăng ký tham gia Activity

**API**: `POST /api/activityparticipant`

**Request Body**:
```json
{
  "activityId": 1,
  "userId": 123  // Lấy từ token (current user)
}
```

**Validation**:
- Kiểm tra user chưa đăng ký (unique constraint)
- Kiểm tra Activity còn chỗ (MaxParticipants)
- Kiểm tra thời gian đăng ký (RegisterDate <= now <= EndRegisterDate)
- Kiểm tra OnlyTeacherCanRegister nếu là Student

**Response**: ActivityParticipantResponseDto

**StarPoint**: Nếu có RegistrationReward, tự động cộng điểm cho user

#### 3.2. Hủy đăng ký (Teacher/Admin)

**API**: `DELETE /api/activityparticipant?participationId=1`

---

### PHASE 4: NỘP BÀI (Submission) - Student

#### 4.1. Tạo Submission

**API**: `POST /api/submission`

**Request Body**:
```json
{
  "activityId": 1,
  "title": "Tác phẩm vẽ tranh mùa xuân của tôi",
  "attachments": [
    {
      "url": "https://cloudinary.com/submission1.jpg",
      "fileName": "tranh_mua_xuan.jpg",
      "fileType": "image/jpeg"
    },
    {
      "url": "https://cloudinary.com/submission1_description.pdf",
      "fileName": "mo_ta.pdf",
      "fileType": "application/pdf"
    }
  ]
}
```

**Validation**:
- User phải là Student
- User phải đã đăng ký Activity (ActivityParticipant)
- Activity phải đang diễn ra hoặc chưa kết thúc

**Response**: SubmissionResponseDto với thông tin user, class, attachments

#### 4.2. Xem Submission của mình

**API**: `GET /api/submission/my-submissions?pageNumber=1&pageSize=10&search=thi`

**API**: `GET /api/submission/activity/{activityId}/my-submission`

#### 4.3. Xem tất cả Submission của Activity

**API**: `GET /api/submission/activity/{Id}?pageNumber=1&pageSize=10&search=thi`

**Quyền**: Student, Teacher, Admin đều xem được

#### 4.4. Cập nhật Submission

**API**: `PUT /api/submission/{id}`

**Request Body**:
```json
{
  "id": 1,
  "title": "Tác phẩm đã chỉnh sửa",
  "attachments": [
    {
      "url": "https://cloudinary.com/submission1_v2.jpg",
      "fileName": "tranh_mua_xuan_v2.jpg",
      "fileType": "image/jpeg"
    }
  ]
}
```

**Validation**: Chỉ owner mới được update

#### 4.5. Xóa Submission

**API**: `DELETE /api/submission/{id}`

**Validation**: Chỉ owner mới được xóa

---

### PHASE 5: CHẤM ĐIỂM (Grading) - Teacher/Admin

#### 5.1. Activity có Grading

Khi tạo Activity với `gradingSettings.criteria`, hệ thống sẽ:
- Set `IsGrade = true`
- Lưu danh sách criteria vào `GradingSettings` (JSON string)

#### 5.2. Chấm điểm Submission

**Lưu ý**: Hiện tại chưa có API riêng cho chấm điểm, nhưng có thể:
- Update trực tiếp `Submission.Score` trong database
- Hoặc tạo API mới: `PUT /api/submission/{id}/grade`

**Future API** (đề xuất):
```json
{
  "score": 85.5,
  "gradingDetails": {
    "Tính sáng tạo (40 điểm)": 35,
    "Kỹ thuật vẽ (30 điểm)": 28,
    "Bố cục và màu sắc (30 điểm)": 22.5
  }
}
```

---

### PHASE 6: TRAO GIẢI THƯỞNG (Awards)

#### 6.1. Trao giải và cộng StarPoint

Khi trao giải cho Submission:
- Cập nhật `ActivityReward` tương ứng
- Tự động cộng StarPoint cho user theo `ActivityReward.StarPoints`

**Lưu ý**: Cần implement logic trao giải (có thể là manual hoặc tự động dựa trên Score)

---

## 🎬 KỊCH BẢN DEMO

### Demo 1: Tạo và quản lý CreativeContest

1. **Teacher đăng nhập**
2. **Tạo Activity**:
   - Title: "Cuộc thi vẽ tranh mùa xuân 2024"
   - SubType: CreativeContest
   - Theme: "Mùa xuân quê hương"
   - Genre: "Vẽ tranh"
   - Có Grading với 3 tiêu chí
   - Có RegistrationReward: 50 điểm
   - Có Awards: Giải nhất (500), Giải nhì (300), Giải ba (200)
3. **Xem danh sách Activity** → Thấy activity vừa tạo
4. **Xem chi tiết Activity** → Thấy đầy đủ thông tin

### Demo 2: Học sinh đăng ký và nộp bài

1. **Student đăng nhập**
2. **Xem danh sách Activity** → Thấy activity "Cuộc thi vẽ tranh"
3. **Đăng ký tham gia**:
   - POST /api/activityparticipant
   - Nhận 50 StarPoint (RegistrationReward)
4. **Nộp bài**:
   - POST /api/submission
   - Upload 2 file: tranh.jpg và mô_tả.pdf
5. **Xem Submission của mình**:
   - GET /api/submission/my-submissions
   - GET /api/submission/activity/{id}/my-submission
6. **Chỉnh sửa Submission** (nếu cần):
   - PUT /api/submission/{id}

### Demo 3: Xem và quản lý Submission

1. **Teacher/Admin đăng nhập**
2. **Xem tất cả Submission của Activity**:
   - GET /api/submission/activity/{id}
   - Thấy danh sách các bài nộp với thông tin user, class, attachments
3. **Chấm điểm** (nếu có grading):
   - Update Score trong database hoặc qua API (nếu có)

### Demo 4: SportsFestival Activity

1. **Tạo SportsFestival Activity**:
   - SubType: SportsFestival
   - SportsCategories: ["Chạy 100m", "Chạy 400m", "Nhảy cao"]
   - CompetitionType: "Mixed"
2. **Học sinh đăng ký** → Tham gia các môn thể thao
3. **Nộp kết quả** (nếu cần) → Tạo Submission

### Demo 5: SeminarWorkshop Activity

1. **Tạo SeminarWorkshop Activity**:
   - SubType: SeminarWorkshop
   - Speakers: Danh sách diễn giả
   - ProgramItems: Chương trình chi tiết
2. **Học sinh/Giáo viên đăng ký** → Tham gia workshop
3. **Không cần nộp bài** (Submission không bắt buộc)

---

## 📊 API ENDPOINTS TÓM TẮT

### Activity APIs

| Method | Endpoint | Role | Mô tả |
|--------|----------|------|-------|
| GET | `/api/activity?pageNumber=1&pageSize=10&search=...` | Student, Teacher, Admin | Lấy danh sách Activity |
| GET | `/api/activity/{id}` | Student, Teacher, Admin | Lấy chi tiết Activity |
| POST | `/api/activity` | Teacher, Admin | Tạo Activity |
| PUT | `/api/activity` | Teacher, Admin | Cập nhật Activity |

### ActivityParticipant APIs

| Method | Endpoint | Role | Mô tả |
|--------|----------|------|-------|
| POST | `/api/activityparticipant` | Student, Teacher, Admin | Đăng ký tham gia |
| DELETE | `/api/activityparticipant?participationId={id}` | Teacher, Admin | Hủy đăng ký |

### Submission APIs

| Method | Endpoint | Role | Mô tả |
|--------|----------|------|-------|
| GET | `/api/submission/activity/{Id}?pageNumber=1&pageSize=10&search=...` | Student, Teacher, Admin | Lấy danh sách Submission của Activity |
| GET | `/api/submission/{id}` | Student, Teacher, Admin | Lấy chi tiết Submission |
| GET | `/api/submission/my-submissions?pageNumber=1&pageSize=10&search=...` | Student, Teacher, Admin | Lấy Submission của mình |
| GET | `/api/submission/activity/{activityId}/my-submission` | Student, Teacher, Admin | Lấy Submission của mình trong Activity |
| POST | `/api/submission` | Student | Tạo Submission |
| PUT | `/api/submission/{id}` | Student, Teacher, Admin | Cập nhật Submission (chỉ owner) |
| DELETE | `/api/submission/{id}` | Student, Teacher, Admin | Xóa Submission (chỉ owner) |

---

## 🔑 CÁC TÍNH NĂNG QUAN TRỌNG

### 1. Grading System
- Activity có thể bật/tắt grading
- Khi bật, cần định nghĩa criteria (tiêu chí chấm điểm)
- Criteria được lưu dưới dạng JSON array trong `GradingSettings`
- Submission có thể có `Score` (decimal)

### 2. StarPoint Rewards
- **RegistrationReward**: Tự động cộng điểm khi đăng ký
- **Awards**: Điểm thưởng khi đạt giải (cần implement logic trao giải)

### 3. Registration Control
- `OnlyTeacherCanRegister`: Nếu true, chỉ Teacher mới đăng ký được
- Kiểm tra thời gian đăng ký (RegisterDate - EndRegisterDate)
- Kiểm tra số lượng (MaxParticipants)

### 4. Submission Management
- Một user chỉ có thể có 1 Submission cho mỗi Activity
- Submission có thể có nhiều Attachments
- Chỉ owner mới được update/delete

### 5. Activity Types & SubTypes
- **ActivityType**: Activity (1) hoặc Event (2)
- **SubType**: SportsFestival, CreativeContest, SeminarWorkshop
- Mỗi SubType có thông tin riêng trong ActivityDetail

---

## ⚠️ LƯU Ý KHI DEMO

1. **Authentication**: Tất cả API đều cần JWT token (trừ login)
2. **Role-based Access**: 
   - Student: Đăng ký, nộp bài, xem
   - Teacher: Tạo Activity, xem tất cả, chấm điểm
   - Admin: Full quyền
3. **Validation**: 
   - Kiểm tra thời gian (StartDate < EndDate, RegisterDate < EndRegisterDate < StartDate)
   - Kiểm tra MaxParticipants
   - Kiểm tra unique constraints
4. **StarPoint**: Tự động cộng điểm khi đăng ký (nếu có RegistrationReward)
5. **File Upload**: Attachments cần upload lên Cloudinary trước, sau đó gửi URL trong Submission

---

## 📝 CHECKLIST DEMO

- [ ] Tạo Activity (CreativeContest) với grading
- [ ] Tạo Activity (SportsFestival)
- [ ] Tạo Activity (SeminarWorkshop)
- [ ] Xem danh sách Activity
- [ ] Xem chi tiết Activity
- [ ] Đăng ký tham gia Activity (Student)
- [ ] Kiểm tra StarPoint được cộng khi đăng ký
- [ ] Nộp bài (Submission) với attachments
- [ ] Xem Submission của mình
- [ ] Xem tất cả Submission của Activity
- [ ] Cập nhật Submission
- [ ] Xóa Submission
- [ ] Test OnlyTeacherCanRegister = true
- [ ] Test validation (thời gian, số lượng, unique)

---

## 🚀 CÁC PHẦN KHÁC (KHÔNG CẦN CHI TIẾT)

### Club
- Quản lý câu lạc bộ
- Activity có thể liên kết với Club (ClubId)

### Post
- Bài đăng trên newsfeed
- Không liên quan trực tiếp đến Activity

### Collection
- Bộ sưu tập
- Không liên quan đến Activity

### User Profile
- Thông tin user
- Hiển thị trong Submission (UserFullName, Class)

### StarPoint System
- Hệ thống điểm thưởng
- Tích hợp với Activity qua RegistrationReward và Awards

---

**Tài liệu này tập trung vào phần Activity. Các phần khác chỉ được đề cập sơ lược.**


