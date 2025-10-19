using EduShpere.Infrastructure.Repositories.SearchAnalytics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EduShpere.Infrastructure.Services
{
    public class TrendingUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TrendingUpdateService> _logger;

        public TrendingUpdateService(IServiceProvider serviceProvider, ILogger<TrendingUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var analyticsRepository = scope.ServiceProvider.GetRequiredService<ISearchAnalyticsRepository>();
                    
                    await analyticsRepository.UpdateTrendingScoresAsync();
                    _logger.LogInformation("Trending scores updated at {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating trending scores");
                }

                // Update every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
