using System;

namespace GoBangladesh.Application.ViewModels;

public class PassengerHistoryVm
{
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string BusNumber { get; set; }
    public string AgentName { get; set; }
    public DateTime TripStartTime { get; set; }
    public DateTime TripEndTime { get; set; }
    public DateTime TransactionTime { get; set; }
}