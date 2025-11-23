using Microsoft.AspNetCore.Mvc;
using EduShpere.MLTrainer.Services;
using EduShpere.Infrastructure;
using EduShpere.Application.Services;
using EduShpere.Domain.Enum;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Controllers;

/// <summary>
/// API Controller cho AI Scheduling System - Đề xuất lịch cho activity mới
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScheduleController : BaseController
{
    private readonly ActivityScheduleService _activityScheduleService;
    private readonly EduShpereDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ScheduleController(
        ActivityScheduleService activityScheduleService,
        EduShpereDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _activityScheduleService = activityScheduleService;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Đề xuất lịch cho activity mới sử dụng AI + OR-Tools
    /// </summary>
    [HttpPost("suggest-for-new-activity")]
    public async Task<IActionResult> SuggestScheduleForNewActivity([FromBody] SuggestScheduleRequest request)
    {
        try
        {
            var result = await _activityScheduleService.SuggestScheduleForNewActivityAsync(
                request.ActivityType,
                request.SubType,
                request.Location,
                request.MaxParticipants,
                request.PreferredStartDate,
                request.PreferredEndDate,
                request.Duration);

            return Ok(new
            {
                success = true,
                isOptimal = result.IsOptimal,
                suggestedSlots = result.SelectedSlots.Select(s => new
                {
                    startTime = s.StartTime,
                    endTime = s.EndTime,
                    location = s.Location,
                    mlScore = s.MLScore,
                    explanation = $"AI dự đoán slot này có {s.MLScore:P0} khả năng thành công dựa trên pattern của các activities tương tự"
                }),
                explanation = result.Explanation,
                objectiveValue = result.ObjectiveValue
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Đề xuất nhiều lựa chọn lịch cho activity mới
    /// </summary>
    [HttpPost("suggest-multiple-options")]
    public async Task<IActionResult> SuggestMultipleScheduleOptions([FromBody] SuggestScheduleRequest request)
    {
        try
        {
            var options = await _activityScheduleService.SuggestMultipleScheduleOptionsAsync(
                request.ActivityType,
                request.SubType,
                request.Location,
                request.MaxParticipants,
                request.PreferredStartDate,
                request.PreferredEndDate,
                request.Duration,
                request.NumberOfOptions ?? 5);

            return Ok(new
            {
                success = true,
                options = options.Select((s, index) => new
                {
                    optionNumber = index + 1,
                    startTime = s.StartTime,
                    endTime = s.EndTime,
                    location = s.Location,
                    mlScore = s.MLScore,
                    explanation = $"Option {index + 1}: AI dự đoán {s.MLScore:P0} khả năng thành công"
                }),
                totalOptions = options.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin về model AI (metrics, version, etc.)
    /// </summary>
    [HttpGet("model-info")]
    public IActionResult GetModelInfo()
    {
        try
        {
            var modelPath = Path.Combine("Data", "activity_schedule_model.zip");
            var modelExists = System.IO.File.Exists(modelPath);

            var info = new
            {
                modelExists = modelExists,
                modelPath = modelPath,
                lastModified = modelExists ? System.IO.File.GetLastWriteTime(modelPath) : (DateTime?)null,
                modelSize = modelExists ? new FileInfo(modelPath).Length : 0,
                description = "ML.NET Activity Schedule Model - Đề xuất lịch cho activity mới dựa trên pattern của activities tương tự"
            };

            return Ok(new { success = true, modelInfo = info });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

/// <summary>
/// Request model cho SuggestScheduleForNewActivity
/// </summary>
public class SuggestScheduleRequest
{
    public ActivityType ActivityType { get; set; }
    public string SubType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int MaxParticipants { get; set; }
    public DateTime PreferredStartDate { get; set; }
    public DateTime PreferredEndDate { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? NumberOfOptions { get; set; }
}

