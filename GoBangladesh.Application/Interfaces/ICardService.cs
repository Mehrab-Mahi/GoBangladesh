using GoBangladesh.Application.DTOs.Card;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;

namespace GoBangladesh.Application.Interfaces;

public interface ICardService
{
    PayloadResponse CardInsert(CardCreateRequest model);
    PayloadResponse CheckCardValidity(string cardNumber);
    Card GetCardDetailByCardNumber(string cardNumber);
    void UpdateCard(Card card);
    void UpdateCardStatus(string cardNumber, string status);
}