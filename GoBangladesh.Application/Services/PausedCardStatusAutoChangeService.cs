using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using GoBangladesh.Application.Util;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GoBangladesh.Application.Services;

public class PausedCardStatusAutoChangeService : BackgroundService
{
    private readonly CronExpression _cronExpression;
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly IServiceProvider _serviceProvider;

    public PausedCardStatusAutoChangeService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _cronExpression = CronExpression.Parse("0 * * * *"); // CRON: minute hour day month dayOfWeek
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
                {
                    await Task.Delay(delay, stoppingToken);
                }

                try
                {
                    await MoveCardStatus(stoppingToken);
                }
                catch
                {
                    return;
                }
            }
        }
    }

    private async Task MoveCardStatus(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var cardRepository = scope.ServiceProvider.GetRequiredService<IRepository<Card>>();

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var cards = await cardRepository
            .GetAll()
            .Where(c => c.Status == CardStatus.Paused && c.LastModifiedTime < sevenDaysAgo)
            .ToListAsync(stoppingToken);

        foreach (var card in cards)
        {
            card.Status = CardStatus.Obsolete;
        }

        await cardRepository.SaveChangesAsync(stoppingToken);
    }
}