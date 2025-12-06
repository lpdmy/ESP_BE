# Moderation Management - Use Cases

## Danh sách các Use Case của tính năng Moderation Management (Quản lý kiểm duyệt)

### 1. **View All Reports** (Admin/Teacher)

- Xem danh sách tất cả các báo cáo nội dung vi phạm
- Hỗ trợ pagination (phân trang)
- Hỗ trợ search (tìm kiếm)
- Hiển thị thông tin: Id, ContentText, AuthorId, ReporterId, ReviewerId, Status, ReviewedAt, ReportedAt, AuthorName, ReporterName
- Sắp xếp theo thời gian báo cáo (mới nhất trước)

### 2. **Create Report** (Student/Teacher/Admin)

- Tạo báo cáo nội dung vi phạm
- Người dùng có thể báo cáo nội dung của người khác
- Validation: Không thể báo cáo nội dung của chính mình
- Thông tin báo cáo: ContentText (nội dung vi phạm), AuthorId (người tạo nội dung), Status (trạng thái)
- Tự động lấy ReporterId từ token (người báo cáo)

### 3. **View User Violations** (Admin/Teacher)

- Xem thống kê vi phạm của người dùng
- Hiển thị danh sách người dùng có nội dung bị báo cáo đã được duyệt (Approved)
- Thông tin hiển thị: AuthorId, AuthorName, ViolationCount (số lần vi phạm), LatestViolation (lần vi phạm gần nhất)
- Sắp xếp theo số lần vi phạm (nhiều nhất trước)
- Hỗ trợ pagination và search theo tên người dùng

### 4. **Create Alert/Warning** (Admin/Teacher)

- Tạo cảnh báo và gửi thông báo cho người dùng vi phạm
- Gửi notification đến người dùng với nội dung cảnh báo
- Thông tin: UserId (người nhận cảnh báo), ContentText (nội dung cảnh báo)
- Notification tự động được tạo với type "system"

### 5. **Update Report Status** (Admin/Teacher)

- Cập nhật trạng thái báo cáo
- Có thể duyệt (Approved) hoặc từ chối (Rejected) báo cáo
- Status = 1: Duyệt báo cáo (Approved)
- Status = 0: Từ chối báo cáo (Rejected)
- Validation: Báo cáo phải tồn tại

## Chi tiết Business Rules

### Report Status

- **Pending**: Báo cáo mới tạo, chưa được xử lý
- **Approved**: Báo cáo được duyệt, nội dung vi phạm được xác nhận
- **Rejected**: Báo cáo bị từ chối, nội dung không vi phạm

### Validation Rules

- Người dùng không thể báo cáo nội dung của chính mình
- Chỉ Admin/Teacher mới có quyền xem và xử lý báo cáo
- Chỉ báo cáo có status "Approved" mới được tính vào thống kê vi phạm
- Thống kê vi phạm chỉ hiển thị người dùng có ít nhất 1 báo cáo đã được duyệt

### Notification

- Khi tạo cảnh báo, hệ thống tự động tạo notification với:
  - Title: "Nhà trường đã gửi cho bạn thông báo \"{ContentText}\""
  - Type: "system"
  - Read: false
  - UserId: Người nhận cảnh báo
