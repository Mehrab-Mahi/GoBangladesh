namespace GoBangladesh.Application.DTOs.Settlement;

public class SettlementSummaryData
{
    public string ReceiverOrganizationId { get; set; }
    public string ReceiverOrganization { get; set; }
    public string SenderOrganizationId { get; set; }
    public string SenderOrganization { get; set; }
    public decimal BusFareAmount { get; set; }
    public decimal ReturnAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal InReviewAmount { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal InvoiceAmount { get; set; }
    public decimal TotalAmount { get; set; }
}