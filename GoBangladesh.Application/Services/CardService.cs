using System;
using System.Collections.Generic;
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
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IRepository<PassengerCardMapping> _passengerCardMappingRepository;
    private readonly IRepository<PassengerCardHistory> _passengerCardHistoryRepository;
    private readonly IBaseRepository _baseRepository;
    private readonly IRepository<Organization> _organizationRepository;
    private readonly IRepository<User> _userRepository;

    public CardService(IRepository<Card> cardRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        IRepository<PassengerCardMapping> passengerCardMappingRepository,
        IRepository<PassengerCardHistory> passengerCardHistoryRepository, 
        IBaseRepository baseRepository,
        IRepository<Organization> organizationRepository, 
        IRepository<User> userRepository)
    {
        _cardRepository = cardRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _passengerCardMappingRepository = passengerCardMappingRepository;
        _passengerCardHistoryRepository = passengerCardHistoryRepository;
        _baseRepository = baseRepository;
        _organizationRepository = organizationRepository;
        _userRepository = userRepository;
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

            if(IfDuplicateCard(model.CardNumber))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Duplicate card number!"
                };
            }

            var card = new Card()
            {
                CardNumber = model.CardNumber,
                Status = string.IsNullOrEmpty(model.Status) ? CardStatus.NotUsed : model.Status,
                Balance = 0,
                OrganizationId = string.IsNullOrEmpty(model.OrganizationId) ? currentUser.OrganizationId : model.OrganizationId
            };

            _cardRepository.Insert(card);
            _cardRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Content = card,
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

    private bool IfDuplicateCard(string cardNumber)
    {
        var card = _cardRepository.GetConditional(c => c.CardNumber == cardNumber);

        return card != null;
    }

    public PayloadResponse CheckCardValidity(string cardNumber)
    {
        var currentUser = _loggedInUserService.GetLoggedInUser();

        var card = _cardRepository
            .GetAll()
            .Where(c => c.CardNumber == cardNumber)
            .Include(c => c.Organization)
            .FirstOrDefault();

        if (card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card not found!"
            };
        }

        var cardPassengerMapping = _passengerCardMappingRepository.GetConditional(c => c.CardId == card.Id);

        if (cardPassengerMapping != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card is already registered!"
            };
        }

        var cardPassengerHistory = _passengerCardHistoryRepository.GetConditional(c => c.CardId == card.Id);

        if (cardPassengerHistory != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card is already registered!"
            };
        }

        if (card.OrganizationId != currentUser.OrganizationId)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card organization is not same as passenger organization!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Card",
            Message = "This card is available!",
            Content = card
        };
    }

    public Card GetCardDetailByCardNumber(string cardNumber)
    {
        var card = _cardRepository
            .GetAll()
            .Where(c => c.CardNumber == cardNumber)
            .Include(c =>c.Organization)
            .FirstOrDefault();

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
        var card = _cardRepository.GetConditional(c => c.CardNumber == cardNumber);

        if (card is { Status: CardStatus.InUse or CardStatus.NotUsed })
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

            if (card.CardNumber != model.CardNumber)
            {
                if (IfDuplicateCard(model.CardNumber))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "Duplicate card number!"
                    };
                }
            }

            card.CardNumber = model.CardNumber;

            if (!string.IsNullOrEmpty(model.OrganizationId))
            {
                if (!string.IsNullOrEmpty(model.OrganizationId) && (model.OrganizationId != card.OrganizationId))
                {
                    UpdateAllPassengersOrg(card.Id, model.OrganizationId);
                }

                card.OrganizationId = model.OrganizationId;
            }

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

    private void UpdateAllPassengersOrg(string cardId, string organizationId)
    {
        var passengerCardMapping = _passengerCardMappingRepository
            .GetAll()
            .Where(p => p.CardId == cardId)
            .Include(p => p.User)
            .FirstOrDefault();


        if (passengerCardMapping is { User: not null })
        {
            var passenger = passengerCardMapping.User;
            passenger.OrganizationId = organizationId;

            _userRepository.Update(passenger);
            _userRepository.SaveChanges();
        }
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var query = $@"
                        select C.*, IIF(u.Id is null, 0, 1) as IsRegistered
                        from Cards c
                                 left join PassengerCardMappings pcm on c.Id = pcm.CardId
                                 left join PassengerCardHistory pch on c.Id = pch.CardId
                                 left join Users u on pcm.UserId = u.Id or pch.UserId = u.Id
                        where c.Id =  '{id}'";
            var card = _baseRepository.Query<CardDataDto>(query).FirstOrDefault();

            if (card == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Card not found",
                    PayloadType = "Card"
                };
            }

            var organization = _organizationRepository
                .GetConditional(o => o.Id == card.OrganizationId);

            if(organization == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Card organization not found",
                    PayloadType = "Card"
                };
            }

            card.Organization = organization;

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Content = card,
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

    public PayloadResponse GetAll(CardDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Card",
                    Message = "Card not found"
                };
            }

            var condition = new List<string>();
            var extraCondition = $@"ORDER BY c.LastModifiedTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Card",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (c.CardNumber like '%{filter.SearchQuery}%' or c.Status like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" c.OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Cards c", whereCondition);

            var cardData = GetAllCardData(whereCondition, extraCondition);

            var allOrg = _organizationRepository.GetAll();

            foreach (var data in cardData)
            {
                data.Organization = allOrg.FirstOrDefault(o => o.Id == data.OrganizationId);
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Content = new { data = cardData, rowCount },
                Message = "Card data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"Card fetching is failed because {ex.Message}!"
            };
        }
    }

    private List<CardDataDto> GetAllCardData(string whereCondition, string extraCondition)
    {
        var query = $@"
                    select C.*,
                           IIF(u.Id is null, 0, 1)                  as IsRegistered,
                           case
                               when u.id is null or c.Status = 'Obsolete' or c.Status = 'Not Used' or c.Status = 'In Use' then null
                               else cast(case
                                             when c.Status = 'Paused' and c.LastModifiedBy = u.Id then 1
                                             else 0 end as bit) end as IsSelfDeactivation
                    from Cards c
                             left join PassengerCardMappings pcm on c.Id = pcm.CardId
                             left join PassengerCardHistory pch on c.Id = pch.CardId
                             left join Users u on pcm.UserId = u.Id or pch.UserId = u.Id
                             {whereCondition} {extraCondition}";

        var data = _baseRepository.Query<CardDataDto>(query);

        return data;
    }

    public void MapUserWithCard(string passengerId, string cardId)
    {
        _passengerCardMappingRepository.Insert(new PassengerCardMapping()
        {
            UserId = passengerId,
            CardId = cardId
        });

        _passengerCardMappingRepository.SaveChanges();
    }

    public Card GetCardDataFromPassengerId(string id)
    {
        var data = _passengerCardMappingRepository
            .GetAll()
            .Where(c => c.UserId == id)
            .Include(c => c.Card)
            .FirstOrDefault();

        return data?.Card;
    }

    public void MapUserWithCardHistory(string passengerId, string cardId)
    {
        _passengerCardHistoryRepository.Insert(new PassengerCardHistory()
        {
            UserId = passengerId,
            CardId = cardId
        });

        _passengerCardHistoryRepository.SaveChanges();
    }

    public PayloadResponse CardInsertForPrivatePassenger(CardCreateRequest model)
    {
        try
        {
            if (IfDuplicateCard(model.CardNumber))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Duplicate card number!"
                };
            }

            var card = new Card()
            {
                CardNumber = model.CardNumber,
                Status = string.IsNullOrEmpty(model.Status) ? CardStatus.NotUsed : model.Status,
                Balance = 0,
                OrganizationId = model.OrganizationId
            };

            _cardRepository.Insert(card);
            _cardRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Card",
                Content = card,
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

    public void UpdateCardOrganization(string passengerId, string organizationId)
    {
        var passengerCardMapping = _passengerCardMappingRepository
            .GetAll()
            .Where(p => p.UserId == passengerId)
            .Include(c => c.Card)
            .FirstOrDefault();

        if (passengerCardMapping == null)
        {
            return;
        }

        var card = passengerCardMapping.Card;

        card.OrganizationId = organizationId;

        _cardRepository.Update(card);
        _cardRepository.SaveChanges();
    }

    public void UnmapUserWithPreviousCard(string passengerId, string cardId)
    {
        var mapping = _passengerCardMappingRepository
            .GetConditional(m => m.UserId == passengerId && m.CardId == cardId);

        if (mapping != null)
        {
            _passengerCardMappingRepository.Delete(mapping.Id);
            _passengerCardMappingRepository.SaveChanges();
        }
    }

    public PayloadResponse CheckCardValidityForRegistration(string cardNumber)
    {
        var card = _cardRepository
            .GetAll()
            .Where(c => c.CardNumber == cardNumber)
            .Include(c => c.Organization)
            .FirstOrDefault();

        if (card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card not found!"
            };
        }

        if (card.Status is CardStatus.Obsolete or CardStatus.Paused)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"You can't register a {card.Status} card!"
            };
        }

        var cardPassengerMapping = _passengerCardMappingRepository.GetConditional(c => c.CardId == card.Id);

        if (cardPassengerMapping != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card is already registered!"
            };
        }

        var cardPassengerHistory = _passengerCardHistoryRepository.GetConditional(c => c.CardId == card.Id);

        if (cardPassengerHistory != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card is already registered!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Card",
            Message = "This card is available!",
            Content = card
        };
    }

    public Card? GetPassengerCardDetailByPassengerId(string passengerId)
    {
        var passengerCardMapping = _passengerCardMappingRepository
            .GetAll()
            .Where(p => p.UserId == passengerId)
            .Include(c => c.Card)
            .FirstOrDefault();

        return passengerCardMapping?.Card;
    }

    public PayloadResponse ActivateCard(CardActivationDto cardActivation)
    {
        if (cardActivation == null || string.IsNullOrEmpty(cardActivation.CardId))
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Invalid card activation request!"
            };
        }

        var card = _cardRepository.GetConditional(c => c.Id == cardActivation.CardId);

        if (card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card not found!"
            };
        }

        if (card.Status != CardStatus.Paused)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"You can't activate a {card.Status} card"
            };
        }

        card.Status = CardStatus.InUse;

        _cardRepository.Update(card);
        _cardRepository.SaveChanges();

        var passengerData = _passengerCardMappingRepository.GetAll()
            .Where(p => p.CardId == card.Id)
            .Include(p => p.User)
            .Select(p => p.User)
            .FirstOrDefault();

        if (passengerData != null)
        {
            passengerData.IsActive = true;

            _userRepository.Update(passengerData);
            _userRepository.SaveChanges();
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Card",
            Message = "Card has been activated successfully!"
        };
    }

    public PayloadResponse DeactivateCard(CardActivationDto cardActivation)
    {
        if (cardActivation == null || string.IsNullOrEmpty(cardActivation.CardId))
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Invalid card activation request!"
            };
        }

        var card = _cardRepository.GetConditional(c => c.Id == cardActivation.CardId);

        if (card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card not found!"
            };
        }

        if (card.Status != CardStatus.InUse)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = $"You can't deactivate a {card.Status} card"
            };
        }

        card.Status = CardStatus.Paused;

        _cardRepository.Update(card);
        _cardRepository.SaveChanges();

        var passengerData = _passengerCardMappingRepository.GetAll()
            .Where(p => p.CardId == card.Id)
            .Include(p => p.User)
            .Select(p => p.User)
            .FirstOrDefault();

        if (passengerData != null)
        {
            passengerData.IsActive = false;

            _userRepository.Update(passengerData);
            _userRepository.SaveChanges();
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Card",
            Message = "Card has been deactivated successfully!"
        };
    }

    public PayloadResponse GetCardDetailByCardNumberForReturn(string cardNumber)
    {
        var card = _cardRepository
            .GetAll()
            .Where(c => c.CardNumber == cardNumber)
            .Include(c =>c.Organization)
            .FirstOrDefault();

        if(card == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card not found!"
            };
        }

        if(card.Status != CardStatus.InUse)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Card",
                Message = "Card status needs to be in use!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Card",
            Content = card, 
            Message = "Card data has been fetched successfully!"
        };
    }
}