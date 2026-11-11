
using System.Text.Json;

namespace DmsQueueMonitor.Worker.Services;

public sealed class NotificationStateStore
{
    private const string StateFile =
        "/app/data/notification-state.json";

    private bool _wasAvailable;

    public NotificationStateStore()
    {
        if (!File.Exists(StateFile))
            return;

        try
        {
            var json = File.ReadAllText(StateFile);

            var state = JsonSerializer.Deserialize<State>(json);

            _wasAvailable = state?.WasAvailable ?? false;
        }
        catch
        {
            _wasAvailable = false;
        }
    }

    public bool WasAvailable => _wasAvailable;

    public async Task SetAvailabilityAsync(
        bool available,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(StateFile)!);

        var json = JsonSerializer.Serialize(
            new State(available));

        await File.WriteAllTextAsync(
            StateFile,
            json,
            cancellationToken);

        _wasAvailable = available;
    }

    private sealed record State(bool WasAvailable);
}
