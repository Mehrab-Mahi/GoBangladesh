namespace GoBangladesh.Application.DTOs.Settlement;

public class PaymentVerificationDto
{
    public string InvoicePaymentId { get; set; }
    public bool IsVerified { get; set; } = true;
}