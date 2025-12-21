# Migration: Add HasAwardedParticipationPoints Column

## Mục đích

Thêm cột `HasAwardedParticipationPoints` vào bảng `Activities` để track xem đã cộng điểm tham gia cho activity chưa, tránh cộng điểm trùng lặp.

## Cách chạy script

### Option 1: Chạy trực tiếp trong SQL Server Management Studio (SSMS)

1. Mở SQL Server Management Studio
2. Kết nối đến database `EduShpereDB`
3. Mở file `Add_HasAwardedParticipationPoints_To_Activity.sql`
4. Execute script (F5)

### Option 2: Chạy bằng sqlcmd

```bash
sqlcmd -S <server_name> -d EduShpereDB -i Add_HasAwardedParticipationPoints_To_Activity.sql
```

### Option 3: Chạy trong Azure Data Studio hoặc tool tương tự

1. Kết nối đến database
2. Mở file script
3. Execute

## Nội dung script

Script sẽ:

1. ✅ Kiểm tra xem cột đã tồn tại chưa (idempotent - có thể chạy nhiều lần an toàn)
2. ✅ Thêm cột `HasAwardedParticipationPoints` với:
   - Type: `BIT`
   - Default: `0` (false)
   - NOT NULL
3. ✅ Tạo index `IX_Activities_HasAwardedParticipationPoints` để tối ưu query performance
4. ✅ Thêm description cho cột

## Index được tạo

Index `IX_Activities_HasAwardedParticipationPoints` được tạo với:
- Column: `HasAwardedParticipationPoints`
- Included columns: `Id`, `EndDate`
- Filter: `WHERE IsDeleted = 0 AND EndDate IS NOT NULL`

Index này giúp tối ưu query trong `AutoAwardAllEndedActivitiesAsync` khi filter activities đã kết thúc nhưng chưa được cộng điểm.

## Optional: Đánh dấu activities cũ

Script có phần optional (đã comment) để đánh dấu tất cả activities đã kết thúc là đã được xử lý. 

**Khi nào nên uncomment:**
- Nếu bạn đã có các activities cũ đã kết thúc và có thể đã được cộng điểm thủ công
- Muốn tránh re-process các activities cũ

**Khi nào KHÔNG nên uncomment:**
- Nếu bạn muốn API tự động cộng điểm cho các activities cũ chưa được cộng điểm

## Kiểm tra sau khi chạy

```sql
-- Kiểm tra cột đã được thêm
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Activities' 
    AND COLUMN_NAME = 'HasAwardedParticipationPoints';

-- Kiểm tra index đã được tạo
SELECT 
    name AS IndexName,
    type_desc AS IndexType
FROM sys.indexes
WHERE object_id = OBJECT_ID('Activities')
    AND name = 'IX_Activities_HasAwardedParticipationPoints';

-- Kiểm tra giá trị mặc định
SELECT 
    COUNT(*) AS TotalActivities,
    SUM(CASE WHEN HasAwardedParticipationPoints = 0 THEN 1 ELSE 0 END) AS NotAwarded,
    SUM(CASE WHEN HasAwardedParticipationPoints = 1 THEN 1 ELSE 0 END) AS Awarded
FROM Activities
WHERE IsDeleted = 0;
```

## Rollback (nếu cần)

Nếu cần rollback, chạy script sau:

```sql
-- Xóa index
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Activities_HasAwardedParticipationPoints' AND object_id = OBJECT_ID('Activities'))
BEGIN
    DROP INDEX [IX_Activities_HasAwardedParticipationPoints] ON [dbo].[Activities];
    PRINT 'Đã xóa index IX_Activities_HasAwardedParticipationPoints';
END
GO

-- Xóa cột
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Activities') AND name = 'HasAwardedParticipationPoints')
BEGIN
    ALTER TABLE [dbo].[Activities]
    DROP COLUMN [HasAwardedParticipationPoints];
    PRINT 'Đã xóa cột HasAwardedParticipationPoints';
END
GO
```

## Lưu ý

- Script là **idempotent** - có thể chạy nhiều lần mà không gây lỗi
- Tất cả activities mới sẽ có giá trị mặc định `HasAwardedParticipationPoints = 0`
- API sẽ tự động set flag = `1` sau khi cộng điểm thành công
- Flag đảm bảo mỗi activity chỉ được cộng điểm 1 lần


