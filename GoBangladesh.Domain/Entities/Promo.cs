using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class Promo : Entity
{
    public string Code { get; set; }
    public string PromoType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MaxDiscountAmount { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string OrganizationId { get; set; }
    public string PassengerStatus { get; set; }
    public string CardStatus { get; set; }
    public int MaxUsagePerCard { get; set; } = 1;
    [ForeignKey("OrganizationId")]
    public Organization Organization { get; set; }
    [NotMapped]
    public List<string> PassengerStatusList { get; set; }
    [NotMapped]
    public List<string> CardStatusList { get; set; }
}