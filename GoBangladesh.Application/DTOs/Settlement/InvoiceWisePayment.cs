using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace GoBangladesh.Application.DTOs.Settlement;

public class InvoiceWisePayment
{
    public string InvoiceNumber { get; set; }
    public List<IFormFile> PaymentProof { get; set; }
    public string SenderAccountId { get; set; }
    public string ReceiverAccountId { get; set; }
}