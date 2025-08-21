namespace GoBangladesh.Application.DTOs.Transaction;

public class RechargeRequest
{
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
}