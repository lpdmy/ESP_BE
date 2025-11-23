# ⚡ Quick Reference - AI Scheduling System

## 🚨 Các Lỗi Thường Gặp & Fix Nhanh

### 1. Type Mismatch Error
```
Error: Concatenated columns should have the same type
Fix: Đảm bảo TẤT CẢ fields trong SchedulePredictionData là float
```

### 2. Package Not Found
```
Error: Unable to find package Microsoft.ML.StandardTrainers
Fix: Xóa package này, không cần thiết
```

### 3. API Not Found
```
Error: 'TransformsCatalog' does not contain definition
Fix: Dùng ML.NET 2.0.1, dùng positional parameters
```

### 4. CreateScope Error
```
Error: 'IServiceProvider' does not contain 'CreateScope'
Fix: Thêm using Microsoft.Extensions.DependencyInjection
```

## 📋 Template Code

### ML Pipeline (Regression)
```csharp
var pipeline = _mlContext.Transforms.Concatenate("Features",
        nameof(Data.Field1),  // float
        nameof(Data.Field2),  // float
        nameof(Data.Field3)   // float
    )
    .Append(_mlContext.Regression.Trainers.Sdca(
        nameof(Data.Label),
        "Features",
        maximumNumberOfIterations: 100));
```

### Background Worker
```csharp
using var scope = _serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
// Use dbContext...
```

### Model Data Class
```csharp
public class MyData
{
    public float Field1 { get; set; }  // ✅ float
    public float Field2 { get; set; }  // ✅ float
    // ❌ KHÔNG dùng int
}
```

## 🎯 Key Points

1. **Tất cả fields = float** khi concatenate
2. **ML.NET 2.0.1** = version ổn định
3. **CreateScope()** cần DI package
4. **Cast values** khi tạo training data: `(float)intValue`

