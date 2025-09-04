namespace GoBangladesh.Application.DTOs.Settlement;

public class SettlementCardData
{
    public decimal TotalAmount { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal InReviewAmount { get; set; }
    public decimal PendingAmount { get; set; }
}