using System.Net.Http.Json;
using System.Text.Json;

namespace DmsQueueMonitor.Worker.Services;

public sealed class TelegramNotifier
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramNotifier> _logger;

    public TelegramNotifier(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramNotifier> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        var token = _configuration["Telegram:BotToken"];
        var chatId = _configuration["Telegram:ChatId"];

        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(chatId))
        {
            throw new InvalidOperationException(
                "Telegram BotToken або ChatId не налаштовано.");
        }

        var url = $"/bot{token}/sendMessage";

        var payload = new
        {
            chat_id = chatId,
            text = message
        };

        using var response = await _httpClient.PostAsJsonAsync(
            url,
            payload,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(
                cancellationToken),
            cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty(
                "ok", out var ok) || !ok.GetBoolean())
        {
            throw new InvalidOperationException(
                "Telegram API не підтвердив надсилання повідомлення.");
        }

        _logger.LogInformation(
            "Telegram notification sent successfully.");
    }
}
