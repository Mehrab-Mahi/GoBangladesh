namespace GoBangladesh.Application.DTOs.TicketChecker;

public class TicketExaminerRechargeStatistics
{
    public int ThisMonthRechargeCount { get; set; }
    public decimal ThisMonthTotalRechargeAmount { get; set; }
    public int TodayRechargeCount { get; set; }
    public decimal TodayTotalRechargeAmount { get; set; }
}