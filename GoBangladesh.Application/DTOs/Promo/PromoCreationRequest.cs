using System;

namespace GoBangladesh.Application.DTOs.Promo;

public class PromoCreationRequest
{
    public string Code { get; set; }
    public string PromoType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MaxDiscountAmount { get; set; }
    public string Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string OrganizationId { get; set; }
    public string PassengerStatus { get; set; }
    public string CardStatus { get; set; }
    public int MaxUsagePerCard { get; set; } = 1;
}