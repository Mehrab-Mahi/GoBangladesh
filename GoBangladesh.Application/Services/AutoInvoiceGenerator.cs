using Cronos;
using GoBangladesh.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoBangladesh.Application.Services;

public class AutoInvoiceGenerator : BackgroundService
{
    private readonly CronExpression _cronExpression;
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly IServiceProvider _serviceProvider;

    public AutoInvoiceGenerator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cronExpression = CronExpression.Parse("0 0 * * 0");
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var next = _cronExpression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds > 0)
                    await Task.Delay(delay, stoppingToken);

                await GenerateInvoicesAsync(stoppingToken);
            }
        }
    }

    private async Task GenerateInvoicesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var invoiceService = scope.ServiceProvider.GetRequiredService<IInvoiceService>();
        var localTime = GetLocalTime();

        await Task.Run(() => invoiceService.GenerateWeeklyInvoices(DateTimeOffset.UtcNow, localTime), stoppingToken);
    }

    private DateTimeOffset GetLocalTime()
    {
        var bdTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, bdTimeZone);
    }
}