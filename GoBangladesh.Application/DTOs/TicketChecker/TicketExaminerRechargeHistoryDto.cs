using System;

namespace GoBangladesh.Application.DTOs.TicketChecker;

public class TicketExaminerRechargeHistoryDto
{
    public string PassengerName { get; set; }
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; }
    public DateTime TransactionTime { get; set; }
}