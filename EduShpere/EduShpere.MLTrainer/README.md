# 🚀 EduShpere AI Scheduling System

Hệ thống AI Scheduling thông minh sử dụng **ML.NET + OR-Tools + Background Worker** để tự động tối ưu hóa lịch cho người dùng.

## 🎯 Tính năng

### 1. **ML.NET - Dự đoán Slot Phù Hợp**
- Model học từ lịch sử tham gia hoạt động của user
- Dự đoán điểm số phù hợp (0-1) cho từng slot thời gian
- Tự động cải thiện khi có dữ liệu mới

### 2. **OR-Tools - Tối Ưu Hóa Lịch**
- Giải quyết bài toán constraint optimization
- Tránh conflict thời gian
- Tối đa hóa ML score tổng thể
- Đảm bảo không quá nhiều activity trong một ngày

### 3. **Background Worker - Tự Động Retrain**
- Tự động phát hiện dữ liệu mới
- Retrain model mỗi 6 giờ (có thể cấu hình)
- Không cần deploy lại khi có dữ liệu mới

## 📁 Cấu trúc Project

```
EduShpere.MLTrainer/
├── Models/
│   └── SchedulePredictionData.cs      # Data model cho ML
├── Services/
│   ├── ScheduleMLService.cs           # ML.NET service
│   ├── ORToolsScheduler.cs            # OR-Tools optimizer
│   └── ScheduleService.cs             # Main service kết hợp ML + OR-Tools
├── Workers/
│   └── ModelRetrainWorker.cs          # Background worker tự động retrain
├── Program.cs                         # Entry point với menu
└── appsettings.json                   # Connection string
```

## 🛠️ Cài đặt

### 1. Restore Packages

```bash
cd ESP_BE/EduShpere/EduShpere.MLTrainer
dotnet restore
```

### 2. Cấu hình Connection String

Chỉnh sửa `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnectionString": "Your-Connection-String-Here"
  }
}
```

## 🚀 Sử dụng

### Chạy ML Trainer

```bash
dotnet run --project EduShpere.MLTrainer
```

Menu sẽ hiển thị:
```
1. Train Activity Classification Model
2. Train Schedule Prediction Model
3. Chạy Background Worker
4. Test Schedule Service
5. Chạy tất cả
```

### 1. Train Activity Classification Model
Train model phân loại activity dựa trên description (model cũ).

### 2. Train Schedule Prediction Model
Train model mới để dự đoán slot thời gian phù hợp:
- Đọc dữ liệu từ `Activities` và `ActivityParticipants`
- Tính toán features: hour, dayOfWeek, activityType, previousCount, preferredHour
- Train regression model với SDCA trainer
- Lưu model vào `Data/schedule_prediction_model.zip`

### 3. Chạy Background Worker
Chạy background service tự động retrain:
- Kiểm tra dữ liệu mới mỗi 6 giờ
- Tự động retrain nếu có thay đổi
- Chạy liên tục cho đến khi Ctrl+C

### 4. Test Schedule Service
Test toàn bộ pipeline:
- Load ML model
- Tạo lịch tối ưu cho user
- Hiển thị kết quả với giải thích AI

## 📊 API Endpoints

Sau khi tích hợp vào EduShpere API:

### POST `/api/schedule/generate-optimal`
Tạo lịch tối ưu cho user.

**Request:**
```json
{
  "startDate": "2024-01-01T00:00:00",
  "endDate": "2024-01-07T23:59:59",
  "activityIds": [1, 2, 3]
}
```

**Response:**
```json
{
  "success": true,
  "isOptimal": true,
  "selectedSlots": [
    {
      "startTime": "2024-01-01T09:00:00",
      "endTime": "2024-01-01T11:00:00",
      "activityId": 1,
      "mlScore": 0.85,
      "explanation": "Slot này được chọn vì AI dự đoán 85% phù hợp với thói quen của bạn"
    }
  ],
  "explanation": "Slot 09:00 ngày Thứ Hai được chọn vì: AI dự đoán 85% phù hợp...",
  "objectiveValue": 850.0
}
```

### GET `/api/schedule/suggest/{activityId}`
Gợi ý slot tốt nhất cho một activity.

**Response:**
```json
{
  "success": true,
  "slot": {
    "startTime": "2024-01-01T09:00:00",
    "endTime": "2024-01-01T11:00:00",
    "activityId": 1,
    "activityTitle": "Giải bóng chuyền",
    "mlScore": 0.85,
    "explanation": "AI gợi ý slot này vì dự đoán 85% phù hợp với thói quen của bạn..."
  }
}
```

### GET `/api/schedule/model-info`
Lấy thông tin về ML model.

## 🧠 Kiến trúc Hệ thống

```
┌─────────────────┐
│   Database      │
│  (Activities)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Background      │
│ Worker          │ ◄─── Tự động retrain mỗi 6h
│ (Retrain)       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  ML.NET         │
│  Training       │
│  Pipeline       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  model.zip      │
│  (Saved Model)  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Schedule       │
│  Service        │
│  (ML + OR)      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  API            │
│  Endpoints      │
└─────────────────┘
```

## 🔥 Điểm Nổi Bật

### 1. **Production-Ready Pipeline**
- Tự động retrain không cần deploy
- Model versioning
- Error handling đầy đủ

### 2. **Explainable AI**
- Giải thích tại sao chọn slot này
- Hiển thị ML score cho từng slot
- User hiểu được quyết định của AI

### 3. **Constraint Optimization**
- Tránh conflict thời gian
- Giới hạn số activity mỗi ngày
- Tối ưu theo nhiều ràng buộc

### 4. **Real-time Learning**
- Model cải thiện khi có dữ liệu mới
- Không cần can thiệp thủ công
- Tự động adapt với hành vi user

## 📈 Model Metrics

Khi train model, bạn sẽ thấy:
```
📈 Model Metrics:
   R² Score: 0.8523
   Loss: 0.0234
   MAE: 0.0456
```

- **R² Score**: Độ chính xác của model (càng gần 1 càng tốt)
- **Loss**: Loss function value (càng nhỏ càng tốt)
- **MAE**: Mean Absolute Error (càng nhỏ càng tốt)

## 🎓 Demo Flow

1. **Train Model**: Chạy option 2 để train schedule prediction model
2. **Test Service**: Chạy option 4 để test với dữ liệu thật
3. **API Integration**: Sử dụng endpoints trong EduShpere API
4. **Background Worker**: Chạy option 3 để tự động retrain

## 💡 Tips

- **Lần đầu train**: Cần ít nhất 50-100 activities có participants để có kết quả tốt
- **Retrain frequency**: Có thể chỉnh trong `ModelRetrainWorker.cs` (mặc định 6h)
- **ML Score threshold**: Có thể filter slots có score < 0.5 nếu muốn
- **OR-Tools constraints**: Có thể thêm constraints trong `ORToolsScheduler.cs`

## 🐛 Troubleshooting

### Model không train được
- Kiểm tra connection string
- Đảm bảo có dữ liệu trong database
- Kiểm tra logs để xem lỗi cụ thể

### OR-Tools không tìm được solution
- Giảm số activity hoặc tăng khoảng thời gian
- Kiểm tra constraints có quá strict không
- Thử giảm `maxActivitiesPerDay`

### Background Worker không chạy
- Kiểm tra connection string
- Đảm bảo đã restore packages
- Kiểm tra logs

## 📚 Tài liệu Tham Khảo

- [ML.NET Documentation](https://docs.microsoft.com/dotnet/machine-learning/)
- [OR-Tools Documentation](https://developers.google.com/optimization)
- [Background Services in .NET](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services)

---

**Made with ❤️ for EduShpere Project**

