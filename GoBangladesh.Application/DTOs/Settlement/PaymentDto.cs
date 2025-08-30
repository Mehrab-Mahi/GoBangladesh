using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Settlement;

public class PaymentDto
{
    public string InvoiceNumber { get; set; }
    public List<IFormFile> PaymentProof { get; set; }
    public string SenderAccountId { get; set; }
    public string ReceiverAccountId { get; set; }
    public decimal Amount { get; set; }
}