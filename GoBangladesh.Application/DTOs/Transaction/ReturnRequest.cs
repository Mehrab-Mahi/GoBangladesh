namespace GoBangladesh.Application.DTOs.Transaction;

public class ReturnRequest
{
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
}