namespace GoBangladesh.Application.DTOs.Settlement;

public class InvoiceWiseSettlementData
{
    public string InvoiceNumber { get; set; }
    public decimal InvoiceTripAmount { get; set; }
    public decimal InvoiceReturnAmount { get; set; }
    public decimal InvoiceDueAmount { get; set; }
}