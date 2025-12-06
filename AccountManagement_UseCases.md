# Account Management - Use Cases

## Danh sách các Use Case của tính năng Account Management (Quản lý tài khoản)

### 1. **View List of Users by Admin**

- Xem danh sách tất cả người dùng trong hệ thống
- Hỗ trợ pagination (phân trang)
- Hỗ trợ search (tìm kiếm theo tên, email, mã học sinh, mã giáo viên)
- Hỗ trợ filter theo trạng thái (Active/Inactive)
- Hỗ trợ filter theo vai trò (Admin/Teacher/Student)
- Hỗ trợ sort (sắp xếp) theo các trường: ID, tên, email, vai trò, lớp, trạng thái
- Hiển thị thông tin: Mã số, Họ tên, Email, Vai trò, Lớp, Trạng thái

### 2. **View User Details by Admin**

- Xem thông tin chi tiết của một người dùng
- Hiển thị thông tin chung: Họ tên, Email, Vai trò, Trạng thái
- Hiển thị thông tin riêng theo vai trò:
  - **Student**: Mã học sinh, Năm nhập học, Ngày sinh, Khối, Lớp, Số điện thoại
  - **Teacher**: Mã giáo viên, Vị trí
  - **Admin**: Chỉ thông tin cơ bản

### 3. **Create User by Admin**

- Tạo người dùng mới trong hệ thống
- Hỗ trợ tạo 3 loại người dùng:
  - **Admin**: Email, Họ, Tên
  - **Student**: Email, Họ, Tên, Số điện thoại, Mã học sinh, Năm nhập học, Ngày sinh, Khối, Lớp
  - **Teacher**: Email, Họ, Tên, Mã giáo viên, Vị trí
- Validation: Email format, required fields, duplicate email/username check
- Tự động tạo One-Time Login token và gửi email

### 4. **Update User by Admin**

- Cập nhật thông tin người dùng
- Có thể cập nhật: Họ, Tên, Email, Số điện thoại, Avatar URL
- Cập nhật thông tin riêng theo vai trò:
  - **Student**: Mã học sinh, Năm nhập học, Ngày sinh, Khối, Lớp
  - **Teacher**: Mã giáo viên, Vị trí
- Email có thể thay đổi (nhưng phải unique)
- Validation: Email format, required fields, duplicate check

### 5. **Delete User by Admin**

- Xóa người dùng khỏi hệ thống (Soft Delete)
- Set Status = 0 (Inactive)
- Người dùng sẽ không thể đăng nhập vào hệ thống
- Có confirmation modal trước khi xóa

### 6. **Activate/Deactivate User by Admin**

- Kích hoạt người dùng (Status = 1)
- Vô hiệu hóa người dùng (Status = 0)
- Có confirmation modal trước khi thay đổi trạng thái
- Người dùng bị vô hiệu hóa sẽ không thể đăng nhập

### 7. **Search Users by Admin**

- Tìm kiếm người dùng theo nhiều tiêu chí:
  - Tên (firstName, lastName)
  - Email
  - Mã học sinh (studentNumber)
  - Mã giáo viên (teacherCode)
- Hỗ trợ real-time search với Enter key
- Kết quả tìm kiếm được phân trang
- Có thể xóa search để hiển thị lại toàn bộ

### 8. **Filter Users by Status**

- Lọc người dùng theo trạng thái:
  - Tất cả trạng thái
  - Hoạt động (Status = 1)
  - Không hoạt động (Status = 0)
- Kết quả lọc được phân trang

### 9. **Filter Users by Role**

- Lọc người dùng theo vai trò:
  - Tất cả vai trò
  - Học sinh (Student)
  - Giáo viên (Teacher)
  - Quản trị viên (Admin)
- Kết quả lọc được phân trang

### 10. **Sort Users by Admin**

- Sắp xếp người dùng theo các trường:
  - Mã số (ID)
  - Họ tên (Name)
  - Email
  - Vai trò (Role)
  - Lớp (Class)
  - Trạng thái (Status)
- Hỗ trợ sort tăng dần (asc) và giảm dần (desc)
- Click vào header để toggle sort direction

### 11. **Import Users from Excel by Admin**

- Nhập danh sách người dùng từ file Excel/CSV
- Hỗ trợ import hàng loạt
- Validation dữ liệu trong file
- Hiển thị kết quả import (thành công/thất bại)
- Navigate đến trang import riêng

### 12. **View User Statistics by Admin**

- Xem thống kê tổng quan về người dùng:
  - Tổng số người dùng
  - Số người dùng hoạt động
  - Số học sinh
  - Số giáo viên
  - Số admin
- Hiển thị dưới dạng cards với số liệu real-time

### 13. **Pagination Management**

- Chọn số lượng items hiển thị trên mỗi trang (5, 10, 20, 50)
- Điều hướng giữa các trang (Previous/Next)
- Hiển thị số trang hiện tại / tổng số trang
- Hiển thị số lượng items đang hiển thị

### 14. **Filter by Academic Year**

- Lọc người dùng theo niên khóa (nếu có)
- Chọn niên khóa từ dropdown
- Áp dụng filter cho danh sách người dùng



