using System;

namespace GoBangladesh.Application.DTOs.Settlement;

public class InvoiceData
{
    public string Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string FromOrganizationId { get; set; }
    public string FromOrganization { get; set; }
    public string ToOrganizationId { get; set; }
    public string ToOrganization { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}