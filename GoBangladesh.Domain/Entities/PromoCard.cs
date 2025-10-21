using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoBangladesh.Domain.Entities;

public class PromoCard : Entity
{
    public string PromoId { get; set; }
    public string CardId { get; set; }
    public string Status { get; set; }
    public decimal UsageAmount { get; set; } = 0;
    public int UsageCount { get; set; } = 0;
    public DateTime UsageDate;
    [ForeignKey("PromoId")]
    public Promo Promo { get; set; }
    [ForeignKey("CardId")]
    public Card Card { get; set; }
}