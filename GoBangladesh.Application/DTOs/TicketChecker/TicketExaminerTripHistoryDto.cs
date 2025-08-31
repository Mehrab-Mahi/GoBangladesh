using System;

namespace GoBangladesh.Application.DTOs.TicketChecker;

public class TicketExaminerTripHistoryDto
{
    public string PassengerName { get; set; }
    public string CardNumber { get; set; }
    public string BusNumber { get; set; }
    public decimal Amount { get; set; }
    public bool IsRunning { get; set; }
    public string TransactionId { get; set; }
    public DateTime TripStartTime { get; set; }
}