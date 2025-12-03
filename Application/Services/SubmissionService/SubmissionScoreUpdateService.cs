using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EduShpere.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduShpere.Application.Services
{
    public class SubmissionScoreUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SubmissionScoreUpdateService> _logger;
        public SubmissionScoreUpdateService(IServiceScopeFactory scopeFactory, ILogger<SubmissionScoreUpdateService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    _logger.LogInformation("Bắt đầu cập nhật điểm lúc: {time}", DateTime.Now);
                    var context = scope.ServiceProvider.GetRequiredService<EduShpereDbContext>();

                    var submissions = context.Submissions
                        .Include(s => s.JuryAssignments)
                        .ToList();

                    foreach (var submission in submissions)
                    {
                        var validScores = submission.JuryAssignments
                            .Where(j => j.TotalScore.HasValue)
                            .Select(j => j.TotalScore.Value)
                            .ToList();

                        submission.Score = validScores.Any()
                            ? validScores.Average()
                            : null;
                    }

                    await context.SaveChangesAsync();

                    _logger.LogInformation("Đã cập nhật {count} submissions.", submissions.Count);
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // chạy mỗi giờ

            }
        }
    }
}
