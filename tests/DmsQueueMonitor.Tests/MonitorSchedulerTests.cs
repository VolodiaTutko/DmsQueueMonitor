
using DmsQueueMonitor.Worker.Scheduling;

namespace DmsQueueMonitor.Tests;

public sealed class MonitorSchedulerTests
{
    private readonly MonitorScheduler _scheduler = new();

    [Theory]
    [InlineData(22, 59, 23, 00)]
    [InlineData(23, 00, 23, 05)]
    [InlineData(23, 04, 23, 05)]
    [InlineData(23, 10, 23, 15)]
    [InlineData(23, 55, 00, 00)]
    [InlineData(23, 59, 00, 00)]
    [InlineData(00, 00, 00, 01)]
    [InlineData(00, 15, 00, 16)]
    [InlineData(00, 29, 00, 30)]
    [InlineData(00, 30, 00, 40)]
    [InlineData(12, 03, 12, 10)]
    [InlineData(22, 50, 23, 00)]
    public void GetNextCheck_ReturnsExpectedTime(
        int hour,
        int minute,
        int expectedHour,
        int expectedMinute)
    {
        var now = new DateTimeOffset(
            2026, 11, 10,
            hour, minute, 0,
            TimeSpan.FromHours(2));

        var next = _scheduler.GetNextCheck(now);

        Assert.Equal(expectedHour, next.Hour);
        Assert.Equal(expectedMinute, next.Minute);
        Assert.True(next > now);
    }

    [Fact]
    public void GetNextCheck_At2359_ReturnsNextDay()
    {
        var now = new DateTimeOffset(
            2026, 11, 10, 23, 59, 0,
            TimeSpan.FromHours(2));

        var next = _scheduler.GetNextCheck(now);

        Assert.Equal(
            new DateOnly(2026, 11, 11),
            DateOnly.FromDateTime(next.Date));

        Assert.Equal(0, next.Hour);
        Assert.Equal(0, next.Minute);
    }

    [Fact]
    public void GetNextCheck_DuringSummer_UsesKyivTimezone()
    {
        var now = new DateTimeOffset(
            2026, 7, 10, 23, 59, 0,
            TimeSpan.FromHours(3));

        var next = _scheduler.GetNextCheck(now);

        Assert.Equal(TimeSpan.FromHours(3), next.Offset);
        Assert.Equal(0, next.Hour);
        Assert.Equal(0, next.Minute);
    }
}
