namespace DmsQueueMonitor.Worker.Scheduling;

public sealed class MonitorScheduler
{
    private readonly TimeZoneInfo _kyivZone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public DateTimeOffset GetKyivNow()
    {
        return TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow, _kyivZone);
    }

    public DateTimeOffset GetNextCheck(
        DateTimeOffset currentTime)
    {
        var now = TimeZoneInfo.ConvertTime(
            currentTime, _kyivZone);

        var nextUtc = new DateTimeOffset(
            now.Year, now.Month, now.Day,
            now.Hour, now.Minute, 0,
            now.Offset).ToUniversalTime()
            .AddMinutes(1);

        for (var i = 0; i < 11; i++)
        {
            var candidate = TimeZoneInfo.ConvertTime(
                nextUtc.AddMinutes(i), _kyivZone);

            var minute = candidate.Minute;

            bool shouldCheck =
                candidate.Hour == 23
                    ? minute % 5 == 0
                    : candidate.Hour == 0 && minute < 30
                        ? true
                        : minute % 10 == 0;

            if (shouldCheck)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            "Could not calculate next check time.");
    }

    public TimeSpan GetDelayUntilNextCheck()
    {
        var now = DateTimeOffset.UtcNow;
        var next = GetNextCheck(now);
        return next.ToUniversalTime() - now;
    }
}
