using System;
using System.Collections.Generic;

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
    public List<string> PassengerStatusList { get; set; }
    public List<string> CardStatusList { get; set; }
    public int MaxUsagePerCard { get; set; } = 1;
}