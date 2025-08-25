using System;

namespace GoBangladesh.Application.DTOs.Settlement;

public class SettlementDetailData
{
    public DateTime TransactionTime { get; set; }
    public string CardNumber { get; set; }
    public string TransactionId { get; set; }
    public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string InvoiceNumber { get; set; }
    public string Status { get; set; }
}