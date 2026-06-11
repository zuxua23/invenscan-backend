using InvenScan.Service.Interfaces;

namespace InvenScan.Service.Implementations;

public class ActivityLogCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ActivityLogCleanupService> _logger;

    public ActivityLogCleanupService(IServiceScopeFactory scopeFactory, ILogger<ActivityLogCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IActivityLogService>();
                var days = await service.GetAutoDeleteDaysAsync();
                if (days > 0)
                {
                    var deleted = await service.CleanupOldLogsAsync(days);
                    _logger.LogInformation("Activity log cleanup: deleted {Count} records older than {Days} days", deleted, days);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Activity log cleanup failed");
            }
        }
    }
}
