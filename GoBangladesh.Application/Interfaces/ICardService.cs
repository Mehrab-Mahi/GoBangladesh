using System.Collections.Generic;
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
    PayloadResponse CheckCardAvailability(string cardNumber);
    PayloadResponse CardUpdate(CardUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse Delete(string id);
    PayloadResponse GetAll(CardDataFilter filter);
    void MapUserWithCard(string passengerId, string cardId);
    Card GetCardDataFromPassengerId(string id);
    void MapUserWithCardHistory(string passengerId, string cardId);
    PayloadResponse CardInsertForPrivatePassenger(CardCreateRequest model);
    void UpdateCardOrganization(string passengerId, string organizationId);
    void UnmapUserWithPreviousCard(string passengerId, string cardId);
    PayloadResponse CheckCardValidityForRegistration(string cardNumber);
    Card? GetPassengerCardDetailByPassengerId(string passengerId);
    PayloadResponse ActivateCard(CardActivationDto cardActivation);
    PayloadResponse DeactivateCard(CardActivationDto cardActivation);
    PayloadResponse GetCardDetailByCardNumberForReturn(string cardNumber);
    void UpdateCardPassengerStatus(string cardNumber, string status);
    List<Card> GetAllCardForByUserId(string userId);
}