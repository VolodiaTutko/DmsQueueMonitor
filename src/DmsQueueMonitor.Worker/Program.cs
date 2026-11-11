using DmsQueueMonitor.Worker;
using DmsQueueMonitor.Worker.Services;
using DmsQueueMonitor.Worker.Scheduling;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<DmsAvailabilityClient>(
    client =>
    {
        client.BaseAddress =
            new Uri("https://cherga.dmsu.gov.ua");

        client.Timeout = TimeSpan.FromSeconds(15);
    });

builder.Services.AddHttpClient<TelegramNotifier>(
    client =>
    {
        client.BaseAddress =
            new Uri("https://api.telegram.org/");

        client.Timeout = TimeSpan.FromSeconds(15);
    });


builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<MonitorScheduler>();

builder.Services.AddSingleton<AvailabilityNotificationService>();

builder.Services.AddSingleton<NotificationStateStore>();

var host = builder.Build();
host.Run();
