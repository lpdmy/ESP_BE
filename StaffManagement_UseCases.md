# Staff Management - Use Cases

## Danh sách các Use Case của tính năng Staff Management

### 1. **View List of Staff by Admin**

- Xem danh sách tất cả nhân sự trong hệ thống
- Hỗ trợ pagination (phân trang)
- Hỗ trợ search (tìm kiếm theo tên, email)
- Hiển thị thông tin: Tên, Email, Quyền hạn, Trạng thái

### 2. **View Staff Details by Admin**

- Xem thông tin chi tiết của một nhân viên
- Hiển thị: Họ tên, Email, Số điện thoại, Quyền hạn được cấp

### 3. **Create Staff by Admin**

- Tạo nhân viên mới trong hệ thống
- Nhập thông tin: Email, Họ, Tên, Số điện thoại, Mật khẩu
- Chọn và cấp quyền hạn cho nhân viên
- Validation: Email format, required fields, permissions

### 4. **Update Staff Information by Admin**

- Cập nhật thông tin nhân viên
- Có thể cập nhật: Họ, Tên, Số điện thoại, Mật khẩu (optional)
- Cập nhật quyền hạn của nhân viên
- Email không thể thay đổi

### 5. **Delete Staff by Admin**

- Xóa nhân viên khỏi hệ thống (Soft Delete)
- Set IsDeleted = true
- Có validation để tránh xóa nhân viên đang có liên kết

### 6. **Recovery Staff by Admin**

- Phục hồi nhân viên đã bị xóa
- Set IsDeleted = false
- Khôi phục quyền truy cập của nhân viên

### 7. **Search Staff by Admin**

- Tìm kiếm nhân sự theo tên hoặc email
- Hỗ trợ real-time search với debounce
- Kết quả tìm kiếm được phân trang

### 8. **Filter Staff by Status**

- Lọc nhân sự theo trạng thái (Active/Deleted)
- Hiển thị số lượng nhân sự theo từng trạng thái

### 9. **Manage Staff Permissions**

- Xem danh sách quyền hạn của nhân viên
- Cấp thêm quyền hạn cho nhân viên
- Thu hồi quyền hạn của nhân viên
- Các quyền có sẵn: MANAGE_CLASS, MANAGE_STAFF, MANAGE_STUDENT, MANAGE_CLUB, etc.

### 10. **View Staff Statistics**

- Xem tổng số nhân sự trong hệ thống
- Xem số lượng quyền hạn có thể cấp
- Thống kê nhân sự đang hoạt động



