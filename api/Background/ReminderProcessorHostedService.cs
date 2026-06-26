using Api.Options;
using Api.Services;
using Microsoft.Extensions.Options;

namespace Api.Background;

public class ReminderProcessorHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<ReminderProcessorOptions> options,
    ILogger<ReminderProcessorHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PollingIntervalSeconds));

        logger.LogInformation(
            "Reminder processor started with polling interval of {IntervalSeconds}s",
            interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                await reminderService.ProcessDueRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in reminder processor loop");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Reminder processor stopped");
    }
}
