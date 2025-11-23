using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EduShpere.MLTrainer.Services;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduShpere.MLTrainer.Workers;

/// <summary>
/// Background Worker tự động retrain model khi có dữ liệu mới
/// </summary>
public class ModelRetrainWorker : BackgroundService
{
    private readonly ILogger<ModelRetrainWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Retrain mỗi 6 giờ
    private DateTime _lastRetrainTime = DateTime.MinValue;

    public ModelRetrainWorker(
        ILogger<ModelRetrainWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Model Retrain Worker đã khởi động");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndRetrainIfNeededAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi trong Model Retrain Worker");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndRetrainIfNeededAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnectionString");

        var optionsBuilder = new DbContextOptionsBuilder<EduShpereDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var dbContext = new EduShpereDbContext(optionsBuilder.Options);

        // Kiểm tra xem có dữ liệu mới không
        var lastActivityTime = await dbContext.Activities
            .Where(a => !a.IsDeleted && a.StartDate.HasValue)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Nếu có dữ liệu mới hoặc chưa train lần nào
        if (lastActivityTime > _lastRetrainTime || _lastRetrainTime == DateTime.MinValue)
        {
            _logger.LogInformation("🔄 Phát hiện dữ liệu mới. Bắt đầu retrain models...");

            // Retrain Activity Schedule Model (cho activity mới)
            var activityScheduleMLService = new ActivityScheduleMLService();
            await activityScheduleMLService.TrainModelAsync(dbContext);
            _logger.LogInformation("✅ Activity Schedule Model đã được retrain");

            _lastRetrainTime = DateTime.UtcNow;
            _logger.LogInformation("✅ Tất cả models đã được retrain thành công");
        }
        else
        {
            _logger.LogDebug("ℹ️ Không có dữ liệu mới. Bỏ qua retrain.");
        }
    }
}

