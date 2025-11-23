using Microsoft.ML.Data;

namespace EduShpere.MLTrainer.Models;

/// <summary>
/// Dữ liệu để train model dự đoán lịch cho activity mới
/// Dựa trên pattern của các activities tương tự đã có
/// </summary>
public class ActivityScheduleData
{
    [LoadColumn(0)]
    public float ActivityType { get; set; } // Category enum

    [LoadColumn(1)]
    public float SubTypeHash { get; set; } // Hash của SubType để convert thành số

    [LoadColumn(2)]
    public float HourOfDay { get; set; } // 0-23

    [LoadColumn(3)]
    public float DayOfWeek { get; set; } // 0-6 (Sunday = 0)

    [LoadColumn(4)]
    public float Month { get; set; } // 1-12

    [LoadColumn(5)]
    public float LocationHash { get; set; } // Hash của Location

    [LoadColumn(6)]
    public float ParticipantCount { get; set; } // Số người tham gia thực tế

    [LoadColumn(7)]
    public float MaxParticipants { get; set; } // Số người tối đa

    [LoadColumn(8)]
    public float SuccessScore { get; set; } // Điểm thành công (0-1): participantCount/maxParticipants
}

/// <summary>
/// Kết quả dự đoán lịch cho activity
/// </summary>
public class ActivitySchedulePrediction
{
    [ColumnName("Score")]
    public float PredictedSuccessScore { get; set; } // Điểm dự đoán thành công (0-1)
}

