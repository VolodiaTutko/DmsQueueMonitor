using System.Net;
using System.Text.Json;
using DmsQueueMonitor.Worker.Scheduling;
using DmsQueueMonitor.Worker.Services;

namespace DmsQueueMonitor.Worker;

public sealed class Worker(
    MonitorScheduler scheduler,
    DmsAvailabilityClient dmsClient,
    AvailabilityNotificationService notificationService,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation("DMS Monitor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = scheduler.GetKyivNow();
                var nextCheck = scheduler.GetNextCheck(now);

                var delay = nextCheck - now;

                logger.LogInformation(
                    "Current Kyiv time: {Now}. " +
                    "Next check: {NextCheck}. " +
                    "Delay: {Delay}",
                    now,
                    nextCheck,
                    delay);

                await Task.Delay(delay, stoppingToken);

                await CheckAvailabilityAsync(stoppingToken);
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode is
                    HttpStatusCode.Forbidden or
                    HttpStatusCode.TooManyRequests)
            {
                logger.LogError(
                    ex,
                    "DMS access restricted. Monitoring stopped.");

                break;
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected monitoring error.");
            }
        }

        logger.LogInformation("DMS Monitor stopped.");
    }

    private async Task CheckAvailabilityAsync(
        CancellationToken cancellationToken)
    {
        var now = scheduler.GetKyivNow();

        var startDate = DateOnly.FromDateTime(now.Date);
        var endDate = startDate.AddDays(27);

        logger.LogInformation(
            "Checking DMS availability at {Time}",
            now);

        var days = await dmsClient.GetAvailableDaysAsync(
            startDate,
            endDate,
            cancellationToken);

        if (days.ValueKind != JsonValueKind.Array)
        {
            logger.LogWarning(
                "Unexpected DMS response format: {Kind}",
                days.ValueKind);

            return;
        }

        var count = days.GetArrayLength();

        logger.LogInformation(
            "DMS returned {Count} calendar entries.",
            count);

        await notificationService.NotifyIfAvailableAsync(
            days,
            cancellationToken);
    }
}
