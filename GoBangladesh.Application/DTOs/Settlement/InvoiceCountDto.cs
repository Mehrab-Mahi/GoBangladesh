namespace GoBangladesh.Application.DTOs.Settlement;

public class InvoiceCountDto
{
    public int PendingInvoices { get; set; }
    public int InReviewInvoices { get; set; }
    public int SettledInvoices { get; set; }
}