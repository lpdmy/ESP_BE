# Weekly Quiz Module - Tài liệu

## Tổng quan

Module Quiz tuần (Weekly Quiz) cho phép giáo viên tạo quiz và học sinh làm bài. Dữ liệu được lưu trong MongoDB.

## Cấu trúc

### MongoDB Collections

1. **WeeklyQuizzes**: Lưu thông tin quiz
   - Id (ObjectId)
   - Title, Description
   - WeekNumber (1-52), Year
   - Questions (array)
   - Deadline, TimeLimitMinutes, MaxScore
   - CreatedByUserId, CreatedByUserName
   - IsActive, CreatedAt, UpdatedAt

2. **QuizSubmissions**: Lưu kết quả làm bài
   - Id (ObjectId)
   - QuizId, StudentId, StudentName
   - WeekNumber, Year (để query nhanh)
   - Answers (array)
   - Score, MaxScore
   - StartedAt, SubmittedAt, TimeSpentSeconds
   - IsGraded

### API Endpoints

#### Teacher/Admin
- `POST /api/weekly-quiz` - Tạo quiz
- `GET /api/weekly-quiz/my-quizzes` - Lấy quiz của mình
- `PUT /api/weekly-quiz/{id}` - Cập nhật quiz
- `DELETE /api/weekly-quiz/{id}` - Xóa quiz
- `GET /api/weekly-quiz/{quizId}/submissions` - Xem kết quả học sinh

#### Student/All
- `GET /api/weekly-quiz/{id}` - Lấy quiz (không có đáp án cho student)
- `GET /api/weekly-quiz/all` - Lấy tất cả quiz
- `GET /api/weekly-quiz/week/{weekNumber}/year/{year}` - Lấy quiz theo tuần/năm
- `POST /api/weekly-quiz/submit` - Nộp bài
- `GET /api/weekly-quiz/{quizId}/my-submission` - Xem kết quả của mình
- `GET /api/weekly-quiz/{quizId}/check-submitted` - Kiểm tra đã làm chưa
- `GET /api/weekly-quiz/my-submissions` - Lịch sử làm quiz

## Business Rules

1. **Weekly Uniqueness**: Mỗi tuần/năm chỉ có 1 quiz active
2. **Deadline**: Quiz đóng sau deadline
3. **One Submission**: Mỗi học sinh chỉ làm 1 lần/quiz
4. **Auto Grading**: MultipleChoice và TrueFalse được chấm tự động
5. **Ownership**: Chỉ người tạo mới sửa/xóa được quiz

## Validation

- WeekNumber: 1-52
- Year: 2020-2100
- Deadline phải sau thời điểm hiện tại
- Questions phải có ít nhất 1 câu
- MultipleChoice phải có ít nhất 2 options
- TimeLimitMinutes: 1-300 phút
- MaxScore: 1-1000 điểm

## Notes

- ShortAnswer được chấm bằng exact match (case-insensitive)
- Có thể cải thiện ShortAnswer bằng fuzzy matching hoặc keyword matching
- MongoDB indexes nên được tạo cho WeekNumber, Year, QuizId, StudentId để tối ưu query

