namespace GoBangladesh.Application.DTOs.Settlement;

public class PaymentVerificationDto
{
    public string InvoiceNumber { get; set; }
    public bool IsVerified { get; set; } = true;
}