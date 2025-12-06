# Activity Management - Use Cases

## Danh sách các Use Case của tính năng Activity Management (Quản lý hoạt động)

## 1. Activity Management

### 1.1. **View List of Activities**

- Xem danh sách tất cả hoạt động
- Hỗ trợ pagination (phân trang)
- Hỗ trợ search (tìm kiếm)
- Hiển thị số lượng người tham gia

### 1.2. **View List of Activities with Filter**

- Xem danh sách hoạt động với bộ lọc nâng cao
- Hỗ trợ filter theo: subType, status, dateFrom, dateTo, organizer, minParticipants, maxParticipants
- Hỗ trợ sort theo các trường (mặc định: StartDate DESC)
- Hỗ trợ pagination và search

### 1.3. **View My Activities**

- Xem danh sách hoạt động của người dùng hiện tại
- Hiển thị trạng thái tham gia (ParticipationStatus)
- Hiển thị thời gian đăng ký (RegisteredAt)
- Hiển thị điểm thưởng (StarPoints)
- Hỗ trợ filter theo status và search
- Hỗ trợ pagination

### 1.4. **View Activity Details**

- Xem thông tin chi tiết của một hoạt động
- Hiển thị đầy đủ thông tin: title, description, dates, location, participants, etc.

### 1.5. **Create Activity** (Teacher/Admin only)

- Tạo hoạt động mới
- Hỗ trợ nhiều loại hoạt động: activity, sports, creative contest, etc.
- Cấu hình thông tin: title, description, dates, location, maxParticipants, etc.
- Cấu hình đăng ký: registerDate, endRegisterDate, registrationSettings
- Cấu hình chấm điểm: gradingSettings, isGrade
- Cấu hình đề bài và nộp bài (cho CreativeContest): problemText, problemFileUrl, submissionDeadline

### 1.6. **Update Activity** (Teacher/Admin only)

- Cập nhật thông tin hoạt động
- Có thể cập nhật các trường: title, description, dates, location, etc.

### 1.7. **Generate Tournament Schedule** (Teacher/Admin only)

- Tạo lịch thi đấu tự động bằng AI cho hoạt động thể thao
- Sử dụng AI service để generate schedule

### 1.8. **Apply Tournament Schedule** (Teacher/Admin only)

- Áp dụng lịch thi đấu đã tạo vào hoạt động

### 1.9. **Train Schedule Model** (Admin only)

- Huấn luyện mô hình ML cho việc tạo lịch thi đấu

## 2. Activity Participant Management

### 2.1. **Register for Activity** (Student/Teacher/Admin)

- Đăng ký tham gia hoạt động
- Tự động lấy UserId từ token
- Validation: activity tồn tại, chưa đăng ký, chưa đầy maxParticipants, trong thời gian đăng ký

### 2.2. **Register Group for Activity** (Student/Teacher/Admin)

- Đăng ký nhóm tham gia hoạt động
- Hỗ trợ đăng ký theo nhóm (group registration)
- Validation: số lượng thành viên trong nhóm, có leader, etc.

### 2.3. **Register Sport for Activity** (Teacher/Admin only)

- Đăng ký hội thao cho hoạt động thể thao
- Đăng ký đội hình (roster) cho các môn thể thao

### 2.4. **Remove Activity Participant** (Teacher/Admin only)

- Xóa người tham gia khỏi hoạt động
- Chỉ Teacher/Admin mới có quyền

### 2.5. **Cancel Registration** (Student/Teacher/Admin)

- Hủy đăng ký tham gia hoạt động
- Người dùng có thể hủy đăng ký của chính mình

### 2.6. **View Sport Rosters** (Student/Teacher/Admin)

- Xem danh sách đội hình (rosters) của các môn thể thao
- Hỗ trợ pagination

## 3. Activity Match Management

### 3.1. **Generate Bracket** (Admin/Teacher)

- Tạo bracket thi đấu (single elimination) cho hoạt động thể thao
- Tự động tạo các trận đấu dựa trên số lượng đội

### 3.2. **View Bracket** (Student/Teacher/Admin)

- Xem bracket thi đấu của một hoạt động
- Filter theo sportId và grade (khối)

### 3.3. **View Matches by Round** (Student/Teacher/Admin)

- Xem danh sách trận đấu theo vòng (round)
- Filter theo activityId, sportId, round, grade

### 3.4. **View Match Details** (Student/Teacher/Admin)

- Xem thông tin chi tiết một trận đấu

### 3.5. **Create Match** (Admin only)

- Tạo trận đấu mới thủ công

### 3.6. **Update Match Result** (Admin/Teacher)

- Cập nhật kết quả trận đấu
- Cập nhật điểm số, đội thắng, etc.

### 3.7. **Update Match** (Admin/Teacher)

- Cập nhật thông tin trận đấu (thời gian, địa điểm, etc.)

### 3.8. **Delete Bracket** (Admin only)

- Xóa bracket thi đấu

### 3.9. **Delete Match** (Admin only)

- Xóa trận đấu

### 3.10. **Get Eligible Class Groups** (Admin/Teacher)

- Lấy danh sách lớp đủ điều kiện tham gia môn thể thao
- Filter theo activityId và sportId

## 4. Submission Management

### 4.1. **View All Submissions by Activity** (Student/Teacher/Admin)

- Xem danh sách tất cả bài nộp của một hoạt động
- Hỗ trợ pagination và search

### 4.2. **View Submissions by User and Activity** (Student/Teacher/Admin)

- Xem danh sách bài nộp của một user trong một hoạt động
- Hỗ trợ pagination và search

### 4.3. **View Ranking by Activity** (Student/Teacher/Admin)

- Xem bảng xếp hạng bài nộp của một hoạt động
- Sắp xếp theo điểm số

### 4.4. **View All Submissions by User** (Student/Teacher/Admin)

- Xem tất cả bài nộp của một user
- Hỗ trợ pagination

### 4.5. **View Submission Details** (Student/Teacher/Admin)

- Xem thông tin chi tiết một bài nộp

### 4.6. **Create Submission** (Student only)

- Tạo bài nộp mới cho hoạt động
- Upload file, nhập title, etc.
- Validation: activity tồn tại, đã đăng ký, trong thời gian nộp bài

### 4.7. **View My Submissions** (Student/Teacher/Admin)

- Xem danh sách bài nộp của người dùng hiện tại
- Hỗ trợ pagination và search

### 4.8. **View My Submission by Activity** (Student/Teacher/Admin)

- Xem bài nộp của người dùng hiện tại cho một hoạt động cụ thể

### 4.9. **Update Submission** (Student/Teacher/Admin)

- Cập nhật bài nộp
- Chỉ chủ sở hữu mới có quyền cập nhật
- Validation: trong thời gian cho phép cập nhật

### 4.10. **View Submission Status** (Student/Teacher/Admin)

- Xem trạng thái nộp bài của người dùng cho một hoạt động
- Kiểm tra: đã nộp chưa, còn thời gian nộp không, etc.

### 4.11. **Delete Submission** (Student/Teacher/Admin)

- Xóa bài nộp
- Chỉ chủ sở hữu mới có quyền xóa

## 5. Jury Management

### 5.1. **View All Jury by Activity** (Student/Teacher/Admin)

- Xem danh sách giám khảo của một hoạt động
- Hỗ trợ search

### 5.2. **Create Jury** (Student/Teacher/Admin)

- Tạo giám khảo mới cho hoạt động
- Thêm user vào danh sách giám khảo

### 5.3. **Delete Jury** (Student/Teacher/Admin)

- Xóa giám khảo khỏi hoạt động

### 5.4. **Assign Jury** (Student/Teacher/Admin)

- Phân công giám khảo chấm bài nộp cụ thể
- Gán giám khảo cho submission

### 5.5. **View Jury Activities** (Student/Teacher/Admin)

- Xem danh sách hoạt động mà user là giám khảo
- Hỗ trợ pagination

### 5.6. **Random Assign Jury** (Student/Teacher/Admin)

- Phân công giám khảo ngẫu nhiên cho các bài nộp
- Tự động phân công dựa trên số lượng giám khảo yêu cầu

### 5.7. **Delete Assign Jury** (Student/Teacher/Admin)

- Xóa phân công giám khảo

### 5.8. **View All Assignments by User** (Student/Teacher/Admin)

- Xem tất cả phân công giám khảo của user
- Hỗ trợ pagination

### 5.9. **View All Assignments by User Not Grading** (Student/Teacher/Admin)

- Xem danh sách phân công giám khảo chưa chấm điểm
- Hỗ trợ pagination

### 5.10. **View All Assignments by User Grading** (Student/Teacher/Admin)

- Xem danh sách phân công giám khảo đã chấm điểm
- Hỗ trợ pagination

### 5.11. **Grade Submission** (Student/Teacher/Admin)

- Chấm điểm bài nộp
- Nhập điểm theo từng tiêu chí (Scores)
- Nhập comment và tổng điểm (TotalScore)
