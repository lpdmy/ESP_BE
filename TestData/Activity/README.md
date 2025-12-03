# Test Data cho Activity Module

Folder này chứa các file test data để test tính năng Create Activity, đặc biệt là feature "Mở Đề" cho Activity có nộp bài.

## Cấu trúc Files

### 1. `test_create_activity_creative_contest_with_problem.json`
- **Mô tả**: CreativeContest có đầy đủ ProblemText, ProblemFileUrl, và SubmissionDeadline
- **Use case**: Test trường hợp đầy đủ nhất của feature "Mở Đề"
- **Đặc điểm**:
  - Có ProblemText (text đề bài)
  - Có ProblemFileUrl (link đến file PDF)
  - Có SubmissionDeadline (hạn nộp bài)
  - SubmissionDeadline nằm giữa StartDate và EndDate

### 2. `test_create_activity_creative_contest_text_only.json`
- **Mô tả**: CreativeContest chỉ có ProblemText, không có file
- **Use case**: Test trường hợp chỉ có text đề bài
- **Đặc điểm**:
  - Có ProblemText
  - ProblemFileUrl = null
  - Có SubmissionDeadline

### 3. `test_create_activity_sports_festival.json`
- **Mô tả**: SportsFestival (không có đề bài)
- **Use case**: Test activity không có submission
- **Đặc điểm**:
  - SubType = "SportsFestival"
  - ProblemText = null
  - ProblemFileUrl = null
  - SubmissionDeadline = null

### 4. `test_create_activity_seminar_workshop.json`
- **Mô tả**: SeminarWorkshop (không có đề bài)
- **Use case**: Test activity không có submission
- **Đặc điểm**:
  - SubType = "SeminarWorkshop"
  - Có Speakers và ProgramItems
  - Không có ProblemText/ProblemFileUrl/SubmissionDeadline

### 5. `test_create_activity_validation_errors.json`
- **Mô tả**: Các test case cho validation errors
- **Use case**: Test các trường hợp lỗi validation
- **Test cases**:
  - SubmissionDeadline < StartDate → Error
  - SubmissionDeadline > EndDate → Error
  - SubmissionDeadline hợp lệ → Success
  - SportsFestival với SubmissionDeadline = null → Success

## Cách sử dụng

### 1. Test với Postman/Insomnia

**Endpoint**: `POST /api/activities`

**Headers**:
```json
{
  "Content-Type": "application/json",
  "Authorization": "Bearer <your-token>"
}
```

**Body**: Copy nội dung từ file JSON tương ứng (chỉ lấy phần `data`)

### 2. Test với cURL

```bash
# Test CreativeContest với đề bài
curl -X POST "https://localhost:7000/api/activities" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-token>" \
  -d @test_create_activity_creative_contest_with_problem.json
```

### 3. Test với Unit Test

Có thể sử dụng các file này làm test data trong unit tests:

```csharp
var testData = JsonSerializer.Deserialize<CreateActivityDto>(
    File.ReadAllText("TestData/Activity/test_create_activity_creative_contest_with_problem.json")
);
```

## Test Scenarios

### Scenario 1: Tạo Activity với đề bài
1. Sử dụng `test_create_activity_creative_contest_with_problem.json`
2. Gọi API Create Activity
3. Kiểm tra:
   - Activity được tạo thành công
   - ProblemText, ProblemFileUrl, SubmissionDeadline được lưu đúng
   - Validation: SubmissionDeadline >= StartDate và <= EndDate

### Scenario 2: Kiểm tra logic "Mở Đề"
1. Tạo Activity với StartDate trong tương lai
2. Gọi API Get Activity Detail
3. Kiểm tra:
   - `isProblemVisible = false`
   - `problemText = null`
   - `problemFileUrl = null`
   - Frontend hiển thị: "Chưa tới thời gian mở đề"
4. Đợi đến StartDate
5. Gọi lại API Get Activity Detail
6. Kiểm tra:
   - `isProblemVisible = true`
   - `problemText` và `problemFileUrl` có giá trị
   - Frontend hiển thị đầy đủ đề bài

### Scenario 3: Validation Errors
1. Sử dụng `test_create_activity_validation_errors.json`
2. Test từng test case
3. Kiểm tra:
   - API trả về error message đúng
   - Activity không được tạo

## Lưu ý

1. **Timezone**: Tất cả dates trong test data đều dùng UTC. Frontend sẽ convert sang VN time (UTC+7) khi hiển thị.

2. **Dates**: Cần cập nhật dates trong test data để phù hợp với thời điểm test:
   - `startDate`: Phải trong tương lai để test logic "Mở Đề"
   - `endDate`: Phải sau `startDate`
   - `submissionDeadline`: Phải nằm giữa `startDate` và `endDate`

3. **File URLs**: Các URL trong test data là ví dụ, cần thay bằng URL thực tế hoặc test với file upload thực sự.

4. **Token**: Cần có token hợp lệ với quyền Admin hoặc Teacher để test Create Activity.

## Expected Results

### Success Cases
- Status Code: 200
- Response có `data` chứa Activity mới tạo
- Message: "Tạo hoạt động thành công"

### Error Cases
- Status Code: 400 (Bad Request)
- Response có `message` chứa error message
- Ví dụ: "Hạn cuối nộp bài phải sau hoặc bằng ngày bắt đầu hoạt động."


