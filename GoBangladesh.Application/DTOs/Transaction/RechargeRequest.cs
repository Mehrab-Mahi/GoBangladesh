namespace GoBangladesh.Application.DTOs.Transaction;

public class RechargeRequest
{
    public string CardNumber { get; set; }
    public int Amount { get; set; }
}