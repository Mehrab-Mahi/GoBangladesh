namespace GoBangladesh.Application.DTOs.Agent;

public class AgentStatistics
{
    public int TotalTransactionsToday { get; set; }
    public decimal TotalAmountToday { get; set; }
    public int TotalTransactionsThisMonth { get; set; }
    public decimal TotalAmountThisMonth { get; set; }
}