using System;
using System.ComponentModel.DataAnnotations.Schema;

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
    public string InvoiceFilePath { get; set; }
    public string PaymentProof { get; set; }
    public string PaymentBy { get; set; }
    public DateTime? PaymentTime { get; set; }
    public string PaymentReceivedBy { get; set; }
    public DateTime? PaymentReceivedTime { get; set; }
    public string SenderAccountId { get; set; }
    public string ReceiverAccountId { get; set; }
    [ForeignKey("FromOrganizationId")]
    public Organization FromOrganization { get; set; }
    [ForeignKey("ToOrganizationId")]
    public Organization ToOrganization { get; set; }
    [ForeignKey("SenderAccountId")]
    public Account SenderAccount { get; set; }
    [ForeignKey("ReceiverAccountId")]
    public Account ReceiverAccount { get; set; }
}