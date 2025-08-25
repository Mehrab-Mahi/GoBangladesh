using System;

namespace GoBangladesh.Domain.Entities;

public class Invoice : Entity
{
    public string InvoiceNumber { get; set; }
    public string FromOrganizationId { get; set; }
    public string ToOrganizationId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}