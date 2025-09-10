namespace GoBangladesh.Application.DTOs.Settlement;

public class SettlementCardData
{
    public decimal TotalAmount { get; set; }
    public decimal InvoiceAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public int TotalReceiverOrganization { get; set; }
    public int TotalSenderOrganization { get; set; }
}