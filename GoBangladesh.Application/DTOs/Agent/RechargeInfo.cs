namespace GoBangladesh.Application.DTOs.Agent;

public class RechargeInfo
{
    public int ThisMonthTransactionCount { get; set; }
    public decimal ThisMonthTotalAmount { get; set; }
    public int TodayTransactionCount { get; set; }
    public decimal TodayTotalAmount { get; set; }
}