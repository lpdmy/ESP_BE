# 📚 Tóm Tắt Phiên Làm Việc - AI Scheduling System

## 🎯 Mục Tiêu Đã Đạt Được

Xây dựng hệ thống **AI Scheduling thông minh** sử dụng:
- **ML.NET** để dự đoán slot thời gian phù hợp
- **OR-Tools** để tối ưu hóa lịch
- **Background Worker** để tự động retrain model

## 📦 Packages Đã Sử Dụng

### ML.NET
- `Microsoft.ML` Version 2.0.1
- **Lưu ý quan trọng**: 
  - Không dùng `Microsoft.ML.StandardTrainers` (không tồn tại)
  - Không dùng `Microsoft.ML.TextAnalytics` (không tồn tại)
  - Tất cả trainers đã được tích hợp vào package chính

### OR-Tools
- `Google.OrTools` Version 9.9.3963

### Entity Framework & Configuration
- `Microsoft.EntityFrameworkCore.SqlServer` Version 8.0.19
- `Microsoft.Extensions.Configuration.Json` Version 8.0.1
- `Microsoft.Extensions.Hosting` Version 8.0.1
- `Microsoft.Extensions.DependencyInjection` Version 8.0.1

## 🏗️ Kiến Trúc Hệ Thống

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

## 🔧 Các Component Đã Tạo

### 1. Models
- **SchedulePredictionData.cs**: Model cho training data
  - **Quan trọng**: Tất cả fields phải là `float` để tránh lỗi type mismatch khi concatenate
  - Fields: UserId, HourOfDay, DayOfWeek, ActivityType, PreviousActivityCount, PreferredHour, Score

### 2. Services
- **ScheduleMLService.cs**: Service train và sử dụng ML model
  - Train model từ database
  - Predict score cho slot thời gian
  - Load/Save model
  
- **ORToolsScheduler.cs**: Service tối ưu hóa lịch
  - Constraint optimization
  - Tránh conflict thời gian
  - Generate explanation

- **ScheduleService.cs**: Service chính kết hợp ML + OR-Tools
  - Generate optimal schedule
  - Suggest best slot

### 3. Workers
- **ModelRetrainWorker.cs**: Background worker tự động retrain
  - Kiểm tra dữ liệu mới mỗi 6 giờ
  - Tự động retrain khi có thay đổi
  - **Lưu ý**: Cần `using Microsoft.Extensions.DependencyInjection` để dùng `CreateScope()`

### 4. Controllers
- **ScheduleController.cs**: API endpoints
  - `POST /api/schedule/generate-optimal`
  - `GET /api/schedule/suggest/{activityId}`
  - `GET /api/schedule/model-info`

## ⚠️ Các Lỗi Đã Gặp Và Cách Sửa

### 1. Lỗi Package Không Tồn Tại
**Lỗi**: `NU1101 Unable to find package Microsoft.ML.StandardTrainers`
**Giải pháp**: Xóa package này, trainers đã được tích hợp vào `Microsoft.ML`

### 2. Lỗi API Không Tồn Tại (ML.NET Version)
**Lỗi**: `CS1061 'TransformsCatalog.TextTransforms' does not contain a definition for 'FeaturizeText'`
**Giải pháp**: 
- Thử nhiều version: 1.7.1, 2.0.1, 3.0.1
- Cuối cùng dùng **ML.NET 2.0.1** (ổn định nhất)
- Sử dụng positional parameters thay vì named parameters

### 3. Lỗi Type Mismatch Khi Concatenate
**Lỗi**: `Concatenated columns should have the same type. Column 'PreferredHour' has type of Single, but expected column type is Int32.`
**Giải pháp**: 
- **Quan trọng**: Thay đổi tất cả fields trong `SchedulePredictionData` từ `int` → `float`
- Cast tất cả giá trị khi tạo training data: `(float)userId`
- Đơn giản hóa pipeline, bỏ các bước convert type không cần thiết

### 4. Lỗi CreateScope
**Lỗi**: `'IServiceProvider' does not contain a definition for 'CreateScope'`
**Giải pháp**: Thêm `using Microsoft.Extensions.DependencyInjection`

### 5. Lỗi RegressionMetrics.Loss
**Lỗi**: `'RegressionMetrics' does not contain a definition for 'Loss'`
**Giải pháp**: Dùng `MeanSquaredError` (MSE) thay vì `Loss`

## 💡 Best Practices Đã Học

### 1. ML.NET Pipeline
```csharp
// ✅ Đúng: Tất cả columns cùng type (float)
var pipeline = _mlContext.Transforms.Concatenate("Features",
        nameof(SchedulePredictionData.UserId),      // float
        nameof(SchedulePredictionData.HourOfDay),  // float
        nameof(SchedulePredictionData.PreferredHour) // float
    )
    .Append(_mlContext.Regression.Trainers.Sdca(...));
```

### 2. Model Data Structure
```csharp
// ✅ Đúng: Tất cả fields là float
public class SchedulePredictionData
{
    public float UserId { get; set; }  // Không dùng int
    public float HourOfDay { get; set; }
    // ...
}
```

### 3. Background Worker Pattern
```csharp
// ✅ Đúng: Sử dụng CreateScope để tạo scoped services
using var scope = _serviceProvider.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<IService>();
```

### 4. API Registration
```csharp
// ✅ Đúng: Register services trong Program.cs
builder.Services.AddScoped<ScheduleMLService>();
builder.Services.AddScoped<ORToolsScheduler>();
builder.Services.AddScoped<ScheduleService>();
```

## 🚀 Cách Sử Dụng

### 1. Train Model
```bash
cd ESP_BE/EduShpere/EduShpere.MLTrainer
dotnet run
# Chọn option 2: Train Schedule Prediction Model
```

### 2. Test Service
```bash
# Chọn option 4: Test Schedule Service
```

### 3. Chạy Background Worker
```bash
# Chọn option 3: Chạy Background Worker
```

### 4. Sử Dụng API
```http
POST /api/schedule/generate-optimal
{
  "startDate": "2024-01-01T00:00:00",
  "endDate": "2024-01-07T23:59:59",
  "activityIds": [1, 2, 3]
}
```

## 📝 Checklist Khi Tạo ML Model Mới

- [ ] Tất cả fields trong model phải cùng type (float)
- [ ] Cast tất cả giá trị khi tạo training data
- [ ] Kiểm tra pipeline không có type mismatch
- [ ] Test với dữ liệu nhỏ trước
- [ ] Lưu model vào đúng path
- [ ] Load model trước khi predict

## 🎓 Kiến Thức Quan Trọng

1. **ML.NET Version**: Dùng 2.0.1 (ổn định, API rõ ràng)
2. **Type Consistency**: Tất cả columns phải cùng type khi concatenate
3. **Background Services**: Cần DI scope để access scoped services
4. **OR-Tools**: Constraint optimization cho scheduling problems
5. **Explainable AI**: Tạo explanation cho mỗi prediction

## 🔗 File Quan Trọng

- `Models/SchedulePredictionData.cs` - Data model (tất cả float!)
- `Services/ScheduleMLService.cs` - ML training & prediction
- `Services/ORToolsScheduler.cs` - Optimization solver
- `Services/ScheduleService.cs` - Main service
- `Workers/ModelRetrainWorker.cs` - Auto retrain
- `Controllers/ScheduleController.cs` - API endpoints
- `Program.cs` - Entry point với menu

## 📚 Tài Liệu Tham Khảo

- [ML.NET Documentation](https://docs.microsoft.com/dotnet/machine-learning/)
- [OR-Tools Documentation](https://developers.google.com/optimization)
- [Background Services](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services)

---

**Ghi chú**: File này được tạo để tham khảo cho các phiên làm việc sau. 
Tất cả các lỗi và giải pháp đã được ghi lại để tránh lặp lại.

