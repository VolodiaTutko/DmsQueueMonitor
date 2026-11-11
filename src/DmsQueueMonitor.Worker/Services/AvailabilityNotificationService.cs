
using System.Text.Json;

namespace DmsQueueMonitor.Worker.Services;

public sealed class AvailabilityNotificationService(
    TelegramNotifier telegram,
    NotificationStateStore stateStore,
    ILogger<AvailabilityNotificationService> logger)
{
    public async Task NotifyIfAvailableAsync(
        JsonElement days,
        CancellationToken cancellationToken)
    {
        if (days.ValueKind != JsonValueKind.Array)
        {
            logger.LogWarning(
                "Unexpected JSON data type: {Type}",
                days.ValueKind);

            return;
        }

        var available = days.GetArrayLength() > 0;

        if (!available)
        {
            if (stateStore.WasAvailable)
            {
                await stateStore.SetAvailabilityAsync(
                    false,
                    cancellationToken);

                logger.LogInformation(
                    "Availability reset.");
            }

            return;
        }

        if (stateStore.WasAvailable)
        {
            logger.LogInformation(
                "Availability already notified.");
            return;
        }

        await telegram.SendMessageAsync(
            "🟢 ДМС Яворів: у календарі з'явилися дані!\n\n" +
            "📍 Яворів, вул. Львівська, 10\n" +
            "📄 Закордонний паспорт / ID\n\n" +
            "Перевір доступність запису:\n" +
            "https://dmsu.gov.ua/services/online.html",
            cancellationToken);

        await stateStore.SetAvailabilityAsync(
            true,
            cancellationToken);

        logger.LogInformation(
            "New availability notification sent.");
    }
}
