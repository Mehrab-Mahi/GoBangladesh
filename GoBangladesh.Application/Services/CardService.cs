using System;
using System.Linq;
using GoBangladesh.Application.DTOs.Card;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class CardService : ICardService
{
    private readonly IRepository<Card> _cardRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;

    public CardService(IRepository<Card> cardRepository,
        IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService)
    {
        _cardRepository = cardRepository;
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
    }

    public PayloadResponse CardInsert(CardCreateRequest model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            if (string.IsNullOrEmpty(currentUser.OrganizationId))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "User is not assigned with any organization!"
                };
            }

            var card = new Card()
            {
                CardNumber = model.CardNumber,
                Status = CardStatus.NotUsed,
                Balance = 0,
                OrganizationId = currentUser.OrganizationId
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

    public void UpdateCardStatus(string cardNumber, string status)
    {
        if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(status))
        {
            return;
        }

        var card = _cardRepository.GetConditional(c => c.CardNumber == cardNumber);

        if(card == null) return;

        card.Status = status;

        _cardRepository.Update(card);
        _cardRepository.SaveChanges();
    }

    public PayloadResponse CheckCardAvailability(string cardNumber)
    {
        var card = _cardRepository.GetConditional(c => c.CardNumber == cardNumber && c.Status == CardStatus.InUse);

        if (card != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Message = "Card is available"
            };
        }

        var passenger = _userRepository.GetConditional(p => p.CardNumber == cardNumber);

        if (passenger != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Message = "Card is available"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = false,
            PayloadType = "Card",
            Message = "Card is not valid!"
        };
    }

    public PayloadResponse CardUpdate(CardUpdateRequest model)
    {
        try
        {
            var card = _cardRepository.GetConditional(c => c.Id == model.Id);

            if (card == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Card not found",
                    PayloadType = "Card"
                };
            }

            card.CardNumber = model.CardNumber;

            _cardRepository.Update(card);
            _cardRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Message = "Card update has been successful!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"Card update has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var card = _cardRepository
                .GetAll()
                .Where(c => c.Id == id)
                .Include(c => c.Organization)
                .FirstOrDefault();

            if (card == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Card not found",
                    PayloadType = "Card"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Message = "Card data has been fetched successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"Card data has been failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var card = _cardRepository.GetConditional(c => c.Id == id);

            if (card == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Card not found",
                    PayloadType = "Card"
                };
            }

            _cardRepository.Delete(card);
            _cardRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Message = "Card deletion has been successful!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"Card deletion has been failed because {ex.Message}!"
            };
        }
    }
}