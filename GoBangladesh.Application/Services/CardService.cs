using System;
using System.Linq;
using GoBangladesh.Application.DTOs.Card;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class CardService : ICardService
{
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;

    public CardService(IRepository<Card> cardRepository,
        IRepository<User> userRepository)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
    }

    public PayloadResponse CardInsert(CardCreateRequest model)
    {
        try
        {
            var card = new Card()
            {
                CardNumber = model.CardNumber,
                CardId = model.CardId,
                Status = model.Status
            };

            _cardRepository.Insert(card);
            _cardRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Message = "Card has been inserted successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"Card insertion has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse CheckCardValidity(string cardNumber)
    {
        var card = _cardRepository.GetConditional(c => c.CardNumber == cardNumber);

        if (card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card not found!"
            };
        }

        var passenger = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.CardNumber == cardNumber);

        if (passenger != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "This card is already in use!"
            };
        }

        if (card.Status == CardStatus.NotUsed)
        {
            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Message = "This card is available!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = false,
            PayloadType = "Card",
            Message = "This card is not available!"
        };
    }

    public Card GetCardDetailByCardNumber(string cardNumber)
    {
        var card = _cardRepository
            .GetConditional(c => c.CardNumber == cardNumber);

        return card;
    }

    public void UpdateCard(Card card)
    {
        _cardRepository.Update(card);
        _cardRepository.SaveChanges();
    }
}