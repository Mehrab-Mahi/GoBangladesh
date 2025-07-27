using GoBangladesh.Application.Util;

namespace GoBangladesh.Application.DTOs.Transaction;

public class ForceStopTripDto
{
    public string CardNumber { get; set; }
    public string TripId { get; set; }
    public string SessionId { get; set; }
    public string TripCloseStatus { get; set; } = TapOutStatus.MobileApp;
}