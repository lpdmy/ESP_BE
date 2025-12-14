# Troubleshooting: Lỗi 500 khi truy cập Swagger

## Nguyên nhân

Lỗi **500 Internal Server Error** khi truy cập `https://localhost:7084/swagger/v1/swagger.json` thường do:

1. **Bảng `ActivityTemplates` chưa tồn tại trong database**
   - Entity Framework cố gắng query bảng này khi khởi động
   - Swagger scan controllers và khởi tạo services, gây lỗi nếu bảng không tồn tại

2. **Lỗi trong code khi khởi động ứng dụng**

## Giải pháp

### Bước 1: Kiểm tra bảng đã tồn tại chưa

Chạy script kiểm tra:
```sql
-- Chạy file: ESP_BE/Scripts/check_activity_template_table.sql
-- Hoặc chạy lệnh:
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ActivityTemplates';
```

### Bước 2: Tạo bảng nếu chưa có

**Cách nhanh nhất (Khuyến nghị):**
```sql
-- Mở SQL Server Management Studio
-- Chạy file: ESP_BE/Scripts/create_activity_template_table.sql
```

**Hoặc dùng EF Core Migration:**
```bash
cd ESP_BE
dotnet ef migrations add AddActivityTemplateTable --project Infrastructure --startup-project EduShpere
dotnet ef database update --project Infrastructure --startup-project EduShpere
```

### Bước 3: Khởi động lại ứng dụng

Sau khi tạo bảng, khởi động lại ứng dụng và truy cập Swagger lại.

## Kiểm tra Logs

Nếu vẫn lỗi, kiểm tra logs trong console để xem lỗi chi tiết:

1. Mở terminal/console nơi chạy ứng dụng
2. Xem error message chi tiết
3. Tìm dòng có chứa "ActivityTemplate" hoặc "ActivityTemplates"

## Lỗi thường gặp

### Lỗi: "Invalid object name 'ActivityTemplates'"
**Nguyên nhân:** Bảng chưa tồn tại trong database
**Giải pháp:** Chạy script `create_activity_template_table.sql`

### Lỗi: "Cannot open database"
**Nguyên nhân:** Connection string sai hoặc database không tồn tại
**Giải pháp:** Kiểm tra `appsettings.json` và connection string

### Lỗi: "The entity type 'ActivityTemplate' requires a primary key"
**Nguyên nhân:** Cấu hình DbContext thiếu
**Giải pháp:** Đã được sửa trong code, rebuild project

## Liên hệ

Nếu vẫn gặp lỗi sau khi đã chạy script SQL, vui lòng:
1. Copy error message đầy đủ từ console
2. Kiểm tra logs trong `logs/` folder (nếu có)
3. Gửi thông tin lỗi để được hỗ trợ

