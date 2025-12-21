# Power Automate API Guide - Tự động cộng StarPoint

## Tổng quan

API này được thiết kế để Power Automate có thể tự động gọi và cộng điểm tham gia (StarPoint) cho các sự kiện đã kết thúc.

## Endpoints

### 1. Tự động cộng điểm cho TẤT CẢ activities đã kết thúc (RECOMMENDED - Daily Job)

**Endpoint:** `POST /api/activity/auto-award-all-ended-activities`

**Mô tả:** Tự động cộng điểm cho tất cả activities đã kết thúc nhưng chưa được cộng điểm. API này được thiết kế để chạy hàng ngày.

**Request Body:** Không cần

**Performance:** 
- Optimized query chỉ lấy các activities cần thiết
- Chỉ query các fields cần thiết (Id, Title, EndDate)
- Filter ngay trong database (đã kết thúc + chưa cộng điểm + có cấu hình điểm thưởng)
- Tự động skip các activities đã được cộng điểm (dùng flag `HasAwardedParticipationPoints`)

**Response Success (200 OK):**
```json
{
  "data": {
    "success": true,
    "totalProcessed": 5,
    "totalAwarded": 125,
    "totalErrors": 0,
    "message": "Đã xử lý 5 activities: 125 người được cộng điểm, 0 lỗi",
    "results": [
      {
        "success": true,
        "activityId": 1,
        "activityTitle": "Lễ hội thể thao 2024",
        "awardedCount": 25,
        "message": "Đã cộng điểm tham gia cho 25 người tham gia thành công",
        "errorMessage": null
      }
      // ... các activities khác
    ]
  },
  "message": "Đã xử lý 5 activities: 125 người được cộng điểm, 0 lỗi",
  "statusCode": 200
}
```

**Response khi không có activity nào cần xử lý:**
```json
{
  "data": {
    "success": true,
    "totalProcessed": 0,
    "totalAwarded": 0,
    "totalErrors": 0,
    "message": "Không có activity nào cần cộng điểm",
    "results": []
  },
  "message": "Đã xử lý 0 activities: 0 người được cộng điểm, 0 lỗi",
  "statusCode": 200
}
```

---

### 2. Cộng điểm cho một Activity

**Endpoint:** `POST /api/activity/{id}/auto-award-participation-points`

**Mô tả:** Tự động cộng điểm tham gia cho một activity cụ thể

**URL Parameters:**
- `id` (int): ID của activity cần cộng điểm

**Request Body:** Không cần

**Response Success (200 OK):**
```json
{
  "data": {
    "success": true,
    "activityId": 1,
    "activityTitle": "Lễ hội thể thao 2024",
    "awardedCount": 25,
    "message": "Đã cộng điểm tham gia cho 25 người tham gia thành công",
    "errorMessage": null
  },
  "message": "Đã cộng điểm thành công",
  "statusCode": 200
}
```

**Response Error (400 Bad Request):**
```json
{
  "data": {
    "success": false,
    "activityId": 1,
    "activityTitle": "Lễ hội thể thao 2024",
    "awardedCount": 0,
    "message": null,
    "errorMessage": "Activity has not ended yet. Cannot award participation points."
  },
  "message": "Activity has not ended yet. Cannot award participation points.",
  "statusCode": 400
}
```

**Các trường hợp lỗi:**
- Activity chưa kết thúc (`EndDate` chưa đến)
- Activity không tồn tại
- Không có cấu hình điểm thưởng (`ActivityRegistrationReward`)
- Không có participants nào có `Status = Joined`

---

### 2. Batch cộng điểm cho nhiều Activities

**Endpoint:** `POST /api/activity/batch-auto-award-participation-points`

**Mô tả:** Tự động cộng điểm tham gia cho nhiều activities cùng lúc

**Request Body:**
```json
{
  "activityIds": [1, 2, 3, 4, 5]
}
```

**Response Success (200 OK):**
```json
{
  "data": {
    "success": true,
    "totalProcessed": 5,
    "totalAwarded": 125,
    "totalErrors": 0,
    "results": [
      {
        "success": true,
        "activityId": 1,
        "activityTitle": "Lễ hội thể thao 2024",
        "awardedCount": 25,
        "message": "Đã cộng điểm tham gia cho 25 người tham gia thành công",
        "errorMessage": null
      },
      {
        "success": true,
        "activityId": 2,
        "activityTitle": "Cuộc thi sáng tạo",
        "awardedCount": 30,
        "message": "Đã cộng điểm tham gia cho 30 người tham gia thành công",
        "errorMessage": null
      }
      // ... các activities khác
    ]
  },
  "message": "Đã xử lý 5 activities: 125 người được cộng điểm, 0 lỗi",
  "statusCode": 200
}
```

**Response Error (400 Bad Request):**
```json
{
  "data": {
    "success": false,
    "totalProcessed": 3,
    "totalAwarded": 50,
    "totalErrors": 1,
    "results": [
      {
        "success": true,
        "activityId": 1,
        "awardedCount": 25,
        "errorMessage": null
      },
      {
        "success": false,
        "activityId": 2,
        "awardedCount": 0,
        "errorMessage": "Activity has not ended yet."
      },
      {
        "success": true,
        "activityId": 3,
        "awardedCount": 25,
        "errorMessage": null
      }
    ]
  },
  "message": "Đã xử lý 3 activities: 50 người được cộng điểm, 1 lỗi",
  "statusCode": 400
}
```

---

## Cách sử dụng trong Power Automate

### Scenario 1: Daily Job - Tự động cộng điểm cho tất cả activities đã kết thúc (RECOMMENDED)

**Đây là cách được khuyến nghị để sử dụng API này.**

1. **Trigger:** Scheduled (chạy hàng ngày, ví dụ: lúc 00:00 hoặc 01:00)
2. **Action:** HTTP Request
   - Method: `POST`
   - URI: `https://your-api-domain.com/api/activity/auto-award-all-ended-activities`
   - Headers: 
     ```
     Content-Type: application/json
     ```
   - Body: Không cần

**Ưu điểm:**
- ✅ Tự động xử lý tất cả activities đã kết thúc
- ✅ Performance tốt (optimized query)
- ✅ Tự động skip activities đã được cộng điểm (dùng flag)
- ✅ Không cần maintain danh sách activities
- ✅ Chạy một lần mỗi ngày là đủ

**Ví dụ Flow:**
```
1. Recurrence (Scheduled)
   - Frequency: Daily
   - Time: 01:00 (sau khi activities của ngày hôm trước đã kết thúc)

2. HTTP Request - Auto Award All Ended Activities
   - Method: POST
   - URI: https://your-api-domain.com/api/activity/auto-award-all-ended-activities

3. Condition - Check Success
   - If success && totalAwarded > 0: Send notification email với số lượng
   - If totalErrors > 0: Send alert email với chi tiết lỗi
```

---

### Scenario 2: Cộng điểm khi một activity kết thúc

1. **Trigger:** Khi một activity có `EndDate` đã qua
2. **Action:** HTTP Request
   - Method: `POST`
   - URI: `https://your-api-domain.com/api/activity/{activityId}/auto-award-participation-points`
   - Headers: 
     ```
     Content-Type: application/json
     ```
   - Body: Không cần

### Scenario 2: Batch cộng điểm cho nhiều activities đã kết thúc

1. **Trigger:** Scheduled (chạy định kỳ, ví dụ: mỗi ngày lúc 00:00)
2. **Action 1:** Query database để lấy danh sách activities đã kết thúc nhưng chưa được cộng điểm
3. **Action 2:** HTTP Request (Batch)
   - Method: `POST`
   - URI: `https://your-api-domain.com/api/activity/batch-auto-award-participation-points`
   - Headers:
     ```
     Content-Type: application/json
     ```
   - Body:
     ```json
     {
       "activityIds": [1, 2, 3, 4, 5]
     }
     ```

### Ví dụ Flow trong Power Automate

```
1. Recurrence (Scheduled)
   - Frequency: Daily
   - Time: 00:00

2. HTTP Request - Get Activities Ended Yesterday
   - Method: GET
   - URI: https://your-api-domain.com/api/activity?status=ended&endDate={yesterday}

3. Parse JSON (Response từ step 2)

4. Compose - Build ActivityIds Array
   - Input: @{body('Parse_JSON')?['data']?['items']?[0]?['id']}

5. HTTP Request - Batch Award Points
   - Method: POST
   - URI: https://your-api-domain.com/api/activity/batch-auto-award-participation-points
   - Body:
     {
       "activityIds": @{outputs('Compose')}
     }

6. Condition - Check Success
   - If success: Send notification email
   - If error: Send alert email
```

---

## Điều kiện để cộng điểm

1. ✅ Activity đã kết thúc (`EndDate < Now`)
2. ✅ **Chưa được cộng điểm** (`HasAwardedParticipationPoints = false`) - **QUAN TRỌNG**
3. ✅ Có cấu hình điểm thưởng (`ActivityRegistrationReward.StarPoints > 0`)
4. ✅ Participant có `Status = Joined` và `IsDeleted = false`

## Cơ chế Flag (HasAwardedParticipationPoints)

- Mỗi activity có một flag `HasAwardedParticipationPoints` để track xem đã cộng điểm chưa
- Flag được set = `true` sau khi cộng điểm thành công
- API sẽ tự động skip các activities đã có flag = `true`
- Điều này đảm bảo:
  - ✅ Không cộng điểm trùng lặp
  - ✅ Performance tốt (không cần check lại activities đã xử lý)
  - ✅ An toàn khi API được gọi nhiều lần

## Lưu ý

- API này **không yêu cầu authentication** để Power Automate có thể gọi dễ dàng
- Nếu cần bảo mật, có thể thêm API key trong header (cần implement thêm)
- API sẽ tự động skip các activities:
  - Chưa kết thúc (`EndDate >= Now`)
  - Đã được cộng điểm (`HasAwardedParticipationPoints = true`)
  - Không có cấu hình điểm thưởng
- Mỗi participant chỉ được cộng điểm 1 lần (logic đã được xử lý trong `AddPointsWithTransactionAsync`)
- **Performance:** API `auto-award-all-ended-activities` được optimize để chỉ query các activities cần thiết, giúp giảm tải database

## Performance Optimization

API `auto-award-all-ended-activities` được tối ưu hóa:

1. **Query Optimization:**
   - Chỉ select các fields cần thiết (Id, Title, EndDate)
   - Filter ngay trong database (không load toàn bộ data)
   - Join với `ActivityRegistrationReward` để filter activities có cấu hình điểm thưởng

2. **Flag Mechanism:**
   - Dùng flag `HasAwardedParticipationPoints` để skip activities đã xử lý
   - Giảm số lượng activities cần query mỗi lần chạy

3. **Batch Processing:**
   - Xử lý từng activity một cách tuần tự để tránh overload
   - Continue processing ngay cả khi có lỗi ở một activity

4. **Recommended Schedule:**
   - Chạy 1 lần mỗi ngày là đủ (ví dụ: 01:00 AM)
   - Không cần chạy nhiều lần vì flag đảm bảo không cộng trùng

## Testing

### Test với Postman/curl

**Single Activity:**
```bash
curl -X POST "https://your-api-domain.com/api/activity/1/auto-award-participation-points" \
  -H "Content-Type: application/json"
```

**Batch Activities:**
```bash
curl -X POST "https://your-api-domain.com/api/activity/batch-auto-award-participation-points" \
  -H "Content-Type: application/json" \
  -d '{
    "activityIds": [1, 2, 3]
  }'
```

---

## Troubleshooting

### Lỗi: "Activity has not ended yet"
- **Nguyên nhân:** Activity chưa đến ngày kết thúc
- **Giải pháp:** Kiểm tra `EndDate` của activity

### Lỗi: "No reward configuration found"
- **Nguyên nhân:** Activity chưa có cấu hình điểm thưởng
- **Giải pháp:** Tạo `ActivityRegistrationReward` với `StarPoints > 0`

### Lỗi: "No participants found"
- **Nguyên nhân:** Không có participants nào có `Status = Joined`
- **Giải pháp:** Kiểm tra danh sách participants của activity

### awardedCount = 0 nhưng success = true
- **Nguyên nhân:** Không có participants hoặc không có cấu hình điểm thưởng
- **Giải pháp:** Đây là trường hợp hợp lệ, không phải lỗi

