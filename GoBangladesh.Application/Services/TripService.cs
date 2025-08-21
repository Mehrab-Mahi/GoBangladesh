using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class TripService : ITripService
{
    private readonly IRepository<Trip> _tripRepository;

    public TripService(IRepository<Trip> tripRepository)
    {
        _tripRepository = tripRepository;
    }

    public Trip? GetRunningTripByCardNumber(string cardId)
    {
        var runningTrip = _tripRepository
            .GetAll()
            .FirstOrDefault(t => t.CardId == cardId && t.IsRunning);
        
        return runningTrip;
    }
}