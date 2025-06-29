using System;

namespace GoBangladesh.Application.DTOs.Transaction;

public class BusFareRequest
{
    public string CardNumber { get; set; }
    public string StaffUserId { get; set; }
    public string TripStartPoint { get; set; }
    public string TripEndPoint { get; set; }
    public DateTime TripStartTime { get; set; }
    public DateTime TripEndTime { get; set; }
}