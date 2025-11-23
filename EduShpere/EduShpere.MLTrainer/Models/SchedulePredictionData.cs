using Microsoft.ML.Data;

namespace EduShpere.MLTrainer.Models;

/// <summary>
/// Dữ liệu để train model dự đoán slot thời gian phù hợp
/// </summary>
public class SchedulePredictionData
{
    [LoadColumn(0)]
    public float UserId { get; set; }

    [LoadColumn(1)]
    public float HourOfDay { get; set; } // 0-23

    [LoadColumn(2)]
    public float DayOfWeek { get; set; } // 0-6 (Sunday = 0)

    [LoadColumn(3)]
    public float ActivityType { get; set; } // Enum value

    [LoadColumn(4)]
    public float PreviousActivityCount { get; set; } // Số activity đã tham gia trong tuần

    [LoadColumn(5)]
    public float PreferredHour { get; set; } // Giờ ưa thích trung bình của user

    [LoadColumn(6)]
    public float Score { get; set; } // Điểm số từ 0-1 (1 = rất phù hợp, 0 = không phù hợp)
}

/// <summary>
/// Kết quả dự đoán
/// </summary>
public class SchedulePrediction
{
    [ColumnName("Score")]
    public float PredictedScore { get; set; } // Điểm dự đoán từ 0-1
}

