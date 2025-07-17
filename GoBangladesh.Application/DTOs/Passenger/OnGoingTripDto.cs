using System;

namespace GoBangladesh.Application.DTOs.Passenger;

public class OnGoingTripDto
{
    public string TripId { get; set; }
    public string CardId { get; set; }
    public string SessionId { get; set; }
    public string StartingLatitude { get; set; }
    public string StartingLongitude { get; set; }
    public DateTime TripStartTime { get; set; }
    public bool IsRunning { get; set; } = true;
    public string BusNumber { get; set; }
    public string BusName { get; set; }
    public string TripStartPlace { get; set; }
    public string TripEndPlace { get; set; }
    public decimal PenaltyAmount { get; set; }
}