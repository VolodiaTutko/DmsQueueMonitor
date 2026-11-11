
using System.Net;
using System.Text.Json;

namespace DmsQueueMonitor.Worker.Services;

public sealed class DmsAvailabilityClient(HttpClient httpClient)
{
    public async Task<JsonElement> GetAvailableDaysAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/v1/days/98/5" +
                  $"?startDate={startDate:yyyy-MM-dd}" +
                  $"&endDate={endDate:yyyy-MM-dd}";

        using var response = await httpClient.GetAsync(
            url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new HttpRequestException(
                "ДМС обмежила частоту запитів.",
                null,
                response.StatusCode);
        }

        response.EnsureSuccessStatusCode();

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(
                cancellationToken),
            cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty(
                "data", out var data) ||
            data.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException(
                "Некоректна відповідь API ДМС.");
        }

        return data.Clone();
    }
}
