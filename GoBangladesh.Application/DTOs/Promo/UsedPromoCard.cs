using System;

namespace GoBangladesh.Application.DTOs.Promo;

public class UsedPromoCard
{
    public string Name { get; set; }
    public string CardNumber { get; set; }
    public string TransactionId { get; set; }
    public DateTime UsedTime { get; set; }
    public string BusNumber { get; set; }
    public decimal UsageAmount { get; set; }
}