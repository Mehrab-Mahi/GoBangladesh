using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GoBangladesh.Domain.Entities;

public class InvoicePayment : Entity
{
    public string InvoiceNumber { get; set; }
    public string PaymentProof { get; set; }
    public string PaymentBy { get; set; }
    public DateTime? PaymentTime { get; set; }
    public string PaymentReceivedBy { get; set; }
    public DateTime? PaymentReceivedTime { get; set; }
    public string SenderAccountId { get; set; }
    public string ReceiverAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    [ForeignKey("SenderAccountId")]
    public Account SenderAccount { get; set; }
    [ForeignKey("ReceiverAccountId")]
    public Account ReceiverAccount { get; set; }
    [ForeignKey("PaymentBy")]
    public User PaymentByUser { get; set; }
    [ForeignKey("PaymentReceivedBy")]
    public User PaymentReceivedByUser { get; set; }
    [NotMapped]
    public List<string> PaymentProofList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PaymentProof))
                return new List<string>();

            return PaymentProof
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToList();
        }
    }
}