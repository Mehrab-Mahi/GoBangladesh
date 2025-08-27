using System;
using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Settlement;

public class InvoiceData
{
    public string Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string FromOrganizationId { get; set; }
    public string ToOrganizationId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public string InvoiceFilePath { get; set; }
    public List<string> PaymentProof { get; set; }
    public string PaymentBy { get; set; }
    public DateTime? PaymentTime { get; set; }
    public string PaymentReceivedBy { get; set; }
    public DateTime? PaymentReceivedTime { get; set; }
    public string SenderAccountId { get; set; }
    public string ReceiverAccountId { get; set; }
    public Domain.Entities.Organization FromOrganization { get; set; }
    public Domain.Entities.Organization ToOrganization { get; set; }
    public Domain.Entities.Account SenderAccount { get; set; }
    public Domain.Entities.Account ReceiverAccount { get; set; }
    public UserDto PaymentByUserData { get; set; }
    public UserDto PaymentReceivedByUserData { get; set; }
}