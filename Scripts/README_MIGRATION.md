# Hướng dẫn tạo Migration cho ActivityTemplate

## ⚠️ QUAN TRỌNG: Phải chạy script SQL trước khi khởi động ứng dụng!

Nếu bạn gặp lỗi **500 Internal Server Error** khi truy cập Swagger, nguyên nhân có thể là bảng `ActivityTemplates` chưa tồn tại trong database.

## Cách 1: Chạy SQL Script trực tiếp (Khuyến nghị)

1. Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio
2. Kết nối đến database của bạn
3. **Kiểm tra trước:** Chạy file `check_activity_template_table.sql` để xem bảng đã tồn tại chưa
4. Nếu chưa có, mở file `create_activity_template_table.sql`
5. Chạy script (F5 hoặc Execute)

Script sẽ tự động:
- Kiểm tra xem bảng đã tồn tại chưa
- Tạo bảng `ActivityTemplates` với đầy đủ columns
- Tạo indexes để tối ưu performance
- Insert 3 template hệ thống mặc định

## Cách 2: Sử dụng Entity Framework Core Migration (Nếu muốn đồng bộ với EF Core)

### Bước 1: Tạo Migration
```bash
cd ESP_BE
dotnet ef migrations add AddActivityTemplateTable --project Infrastructure --startup-project EduShpere
```

### Bước 2: Xem Migration được tạo
Kiểm tra file trong `ESP_BE/Infrastructure/Migrations/` có tên dạng `YYYYMMDDHHMMSS_AddActivityTemplateTable.cs`

### Bước 3: Apply Migration vào Database
```bash
dotnet ef database update --project Infrastructure --startup-project EduShpere
```

### Bước 4: Insert dữ liệu mẫu (nếu cần)
Chạy phần INSERT trong file `create_activity_template_table.sql` hoặc tạo migration riêng cho seed data.

## Lưu ý

- **Cách 1 (SQL Script)**: Nhanh, đơn giản, phù hợp khi chỉ cần tạo bảng
- **Cách 2 (EF Core Migration)**: Tốt hơn khi muốn đồng bộ với codebase và quản lý migrations tập trung

## Kiểm tra sau khi chạy

```sql
-- Kiểm tra bảng đã được tạo
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ActivityTemplates';

-- Kiểm tra dữ liệu
SELECT * FROM ActivityTemplates WHERE IsDeleted = 0;

-- Kiểm tra indexes
EXEC sp_helpindex 'ActivityTemplates';
```

