using GoBangladesh.Application.DTOs.Promo;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces;

public interface IPromoService
{
    PayloadResponse PromoInsert(PromoCreationRequest model);
    PayloadResponse GetUserPromo(string status, int pageNo, int pageSize);
    PayloadResponse ApplyPromo(ApplyPromoRequest applyPromoRequest);
    bool IfAnyPromoAvailableByCardId(string cardId);
    decimal GetPromoAmount(PromoCard promoCard, decimal fare);
    void MarkPromoAsUsedAndUpdateUsageAmount(PromoCard promoCard, decimal promoAmount);
    PromoCard GetPromoCardByCardId(string cardId);
    PayloadResponse GetAll(PromoDataFilter filter);
    void UpdatePromoUsageAmount(string promoCardId, decimal promoAmount);
    PayloadResponse GetById(string id);
    PayloadResponse GetUsedCardByPromoId(string id, int pageNo, int pageSize);
}