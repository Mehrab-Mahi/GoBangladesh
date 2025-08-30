using System.Collections.Generic;

namespace GoBangladesh.Application.DTOs.Settlement;

public class InvoiceWisePaymentDto
{
    public List<PaymentDto> PaymentData { get; set; }
}