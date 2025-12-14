# Hướng dẫn nhanh: Tạo dữ liệu Template

## Bước 1: Chạy Script SQL

**QUAN TRỌNG:** Bạn cần chạy script SQL để tạo bảng và dữ liệu mẫu trước khi sử dụng tính năng Template.

### Cách 1: Chạy trực tiếp trong SQL Server Management Studio (SSMS)

1. Mở SQL Server Management Studio
2. Kết nối đến database của bạn
3. Mở file: `ESP_BE/Scripts/create_activity_template_table.sql`
4. Chạy toàn bộ script (F5)
5. Kiểm tra kết quả:
   - Nếu thành công, bạn sẽ thấy message: "Hoàn thành script tạo bảng ActivityTemplates!"
   - Script sẽ tự động tạo 3 template mặc định:
     - Hội thảo Chuyên đề (SeminarWorkshop)
     - Cuộc thi Sáng tạo (CreativeContest)
     - Hội thao (SportsFestival)

### Cách 2: Kiểm tra xem đã có dữ liệu chưa

Chạy script: `ESP_BE/Scripts/check_activity_template_table.sql`

Nếu thấy:
- ✅ Bảng ActivityTemplates đã tồn tại
- 📊 Số lượng templates hiện có: 3 (hoặc nhiều hơn)

Thì bạn đã có dữ liệu, không cần chạy lại.

## Bước 2: Kiểm tra Frontend

1. Khởi động lại Backend (nếu đã chạy)
2. Mở Frontend
3. Vào trang "Tạo Hoạt Động Mới"
4. Chọn "Chọn một Mẫu có sẵn"
5. Bạn sẽ thấy 3 template hiển thị

## Troubleshooting

### Nếu không thấy template nào:

1. **Kiểm tra Console (F12):**
   - Xem có lỗi API không
   - Kiểm tra response từ API `/api/activity-template`

2. **Kiểm tra Database:**
   ```sql
   SELECT * FROM ActivityTemplates WHERE IsDeleted = 0;
   ```
   - Nếu không có dữ liệu, chạy lại script `create_activity_template_table.sql`

3. **Kiểm tra Backend:**
   - Đảm bảo Backend đang chạy
   - Kiểm tra Swagger: `https://localhost:7084/swagger`
   - Test API: `GET /api/activity-template`

### Nếu template không load dữ liệu vào form:

1. **Mở Console (F12)**
2. **Kiểm tra logs:**
   - `✅ Saved template data to sessionStorage:` - Xem dữ liệu đã được lưu chưa
   - `📥 Loading prefilled data:` - Xem dữ liệu đã được load chưa
   - `📥 Parsed fields:` - Xem các field đã được parse đúng chưa

3. **Kiểm tra sessionStorage:**
   - Mở DevTools > Application > Session Storage
   - Tìm key `prefilledActivityData`
   - Xem giá trị có đúng không

## Cấu trúc dữ liệu Template

Mỗi template có:
- **Name**: Tên template
- **Description**: Mô tả
- **SubType**: Loại hoạt động (SeminarWorkshop, CreativeContest, SportsFestival)
- **PrefillData**: JSON chứa dữ liệu điền sẵn:
  ```json
  {
    "title": "Hội thảo: [Chủ đề]",
    "description": "Hội thảo chuyên đề về...",
    "subType": "SeminarWorkshop",
    "location": "Hội trường",
    "organizer": "Phòng Đào tạo"
  }
  ```
- **Checklist**: Mảng các công việc:
  ```json
  ["Đặt chỗ hội trường", "Thiết kế banner", "Gửi thông báo", ...]
  ```

