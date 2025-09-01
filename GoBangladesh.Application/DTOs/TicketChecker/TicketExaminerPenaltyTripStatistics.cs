namespace GoBangladesh.Application.DTOs.TicketChecker;

public class TicketExaminerPenaltyTripStatistics
{
    public int ThisMonthPenaltyTripCount { get; set; }
    public decimal ThisMonthTotalPenaltyTripAmount { get; set; }
    public int TodayPenaltyTripCount { get; set; }
    public decimal TodayTotalPenaltyTripAmount { get; set; }
}