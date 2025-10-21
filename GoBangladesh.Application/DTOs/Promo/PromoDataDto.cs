namespace GoBangladesh.Application.DTOs.Promo;

public class PromoDataDto : Domain.Entities.Promo
{
    public int EligibleCards { get; set; }
    public decimal PromoUsageAmount { get; set; }
    public decimal NumberOfPromoUsed { get; set; }
}