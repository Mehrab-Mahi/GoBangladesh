using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using GoBangladesh.Application.DTOs.Transaction;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GoBangladesh.Application.Services;

public class TripAutoCloseService : BackgroundService
{
    private readonly CronExpression _cronExpression;
    private readonly TimeZoneInfo _timeZoneInfo;
    private readonly IServiceProvider _serviceProvider;

    public TripAutoCloseService(IServiceProvider serviceProvider)
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
                    await CloseTripsAsync(stoppingToken);
                }
                catch
                {
                    return;
                }
            }
        }
    }

    private async Task CloseTripsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
        var tripRepository = scope.ServiceProvider.GetRequiredService<IRepository<Trip>>();

        var trips = tripRepository
            .GetAll()
            .Where(t => t.IsRunning && EF.Functions.DateDiffHour(t.TripStartTime, DateTime.UtcNow) >= 8.00)
            .Include(t => t.Card)
            .ToList();

        foreach (var trip in trips)
        {
            await Task.Run(() => transactionService.ForceTripStop(new ForceStopTripDto()
            {
                CardNumber = trip.Card.CardNumber,
                TripId = trip.Id,
                SessionId = trip.SessionId,
                TripCloseStatus = TapOutStatus.TimeOut
            }), stoppingToken);
        }
    }
}