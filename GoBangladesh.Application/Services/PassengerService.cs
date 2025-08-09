using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.Util;
using Microsoft.EntityFrameworkCore;
using GoBangladesh.Application.DTOs.Card;

namespace GoBangladesh.Application.Services;

public class PassengerService : IPassengerService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IRepository<Trip> _tripRepository;
    private readonly ICardService _cardService;
    private readonly IBaseRepository _baseRepository;
    private readonly IRepository<Organization> _organizationRepository;

    public PassengerService(IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService, 
        IRepository<Trip> tripRepository,
        ICardService cardService,
        IBaseRepository baseRepository, 
        IRepository<Organization> organizationRepository)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _tripRepository = tripRepository;
        _cardService = cardService;
        _baseRepository = baseRepository;
        _organizationRepository = organizationRepository;
    }

    public PayloadResponse PassengerInsert(PassengerCreateRequest user)
    {
        if (IfDuplicateUser(user))
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Passenger Creation",
                Content = null,
                Message = "Duplicate user!"
            };
        }

        var card = new Card();

        if (string.IsNullOrEmpty(user.UserType))
        {
            card = _cardService.GetCardDetailByCardNumber(user.CardNumber);
            user.OrganizationId = string.IsNullOrEmpty(user.OrganizationId) ? card.OrganizationId : user.OrganizationId;
            user.UserType = card.Organization.OrganizationType;
            _cardService.UpdateCardStatus(user.CardNumber, CardStatus.InUse);
        }

        if (user.UserType == UserTypes.Private)
        {
            card = _cardService.GetCardDetailByCardNumber(user.CardNumber);

            if (card is null)
            {
                var cardInsertRequest = new CardCreateRequest()
                {
                    CardNumber = user.CardNumber,
                    OrganizationId = user.OrganizationId,
                    Status = CardStatus.InUse
                };
                card = _cardService.CardInsertForPrivatePassenger(cardInsertRequest).Content;
            }
            else
            {
                var cardValidity = _cardService.CheckCardValidityForRegistration(user.CardNumber);

                if (!cardValidity.IsSuccess)
                {
                    return cardValidity;
                }

                if (user.OrganizationId != card.OrganizationId)
                {
                    return new PayloadResponse
                    {
                        IsSuccess = false,
                        PayloadType = "Passenger Creation",
                        Content = null,
                        Message = "Organization is not same!"
                    };
                }

                _cardService.UpdateCardStatus(user.CardNumber, CardStatus.InUse);
            }
        }

        if (user.UserType == UserTypes.Public)
        {
            card = _cardService.GetCardDetailByCardNumber(user.CardNumber);

            if (card is null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Passenger Creation",
                    Content = null,
                    Message = "Card not found!"
                };
            }

            if (user.OrganizationId != card.OrganizationId)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Passenger Creation",
                    Content = null,
                    Message = "Organization is not same!"
                };
            }
            _cardService.UpdateCardStatus(user.CardNumber, CardStatus.InUse);
        }

        try
        {
            var model = new User()
            {
                Name = user.Name,
                EmailAddress = user.EmailAddress,
                DateOfBirth = user.DateOfBirth,
                MobileNumber = user.MobileNumber,
                Address = user.Address,
                Gender = user.Gender,
                UserType = user.UserType,
                PassengerId = user.PassengerId,
                OrganizationId = user.OrganizationId,
            };

            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (string.IsNullOrEmpty(user.Password))
            {
                user.Password = "123";
            }

            model.PasswordHash = _commonService.GetPasswordHash(user.Password);
            model.ImageUrl = _commonService.UploadAndGetImageUrl(user.ProfilePicture, "ProfilePicture");
            model.CreatedBy = currentUser is null ? "" : currentUser.Id;
            model.LastModifiedBy = currentUser is null ? "" : currentUser.Id;

            _userRepository.InsertWithUserData(model);
            _userRepository.SaveChanges();

            _cardService.MapUserWithCard(model.Id, card!.Id);

            return new PayloadResponse
            {
                IsSuccess = true,
                PayloadType = "Passenger Creation",
                Content = null,
                Message = "Passenger Creation has been successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Passenger Creation",
                Content = null,
                Message = $"Passenger Creation become unsuccessful because {ex.Message}"
            };
        }
    }

    public PayloadResponse UpdatePassenger(PassengerUpdateRequest user)
    {
        var model = _userRepository.GetConditional(u => u.Id == user.Id);
        try
        {
            if (user.MobileNumber != model.MobileNumber)
            {
                if (IfDuplicateMobileNumber(user.MobileNumber))
                {
                    return new PayloadResponse
                    {
                        IsSuccess = false,
                        PayloadType = "Passenger Update",
                        Content = null,
                        Message = "Passenger with the mobile number already exists!"
                    };
                }
            }
            
            if (user.EmailAddress != model.EmailAddress && !string.IsNullOrEmpty(user.EmailAddress))
            {
                if (IfDuplicateEmail(user.EmailAddress))
                {
                    return new PayloadResponse
                    {
                        IsSuccess = false,
                        PayloadType = "Passenger Update",
                        Content = null,
                        Message = "Passenger with the email already exists!"
                    };
                }
            }

            if (!string.IsNullOrEmpty(user.OrganizationId) && (user.OrganizationId != model.OrganizationId))
            {
                _cardService.UpdateCardOrganization(user.Id, user.OrganizationId);
            }

            model.Name = user.Name;
            model.DateOfBirth = user.DateOfBirth;
            model.MobileNumber = user.MobileNumber;
            model.EmailAddress = user.EmailAddress;
            model.Address = user.Address;
            model.Gender = user.Gender;
            model.PassengerId = user.PassengerId;
            model.OrganizationId = user.OrganizationId;
            model.UserType = user.UserType;

            if (user.ProfilePicture is { Length: > 0 })
            {
                _commonService.DeleteFile(model.ImageUrl);
                model.ImageUrl = _commonService
                    .UploadAndGetImageUrl(user.ProfilePicture, "ProfilePicture");
            }

            _userRepository.Update(model);
            _userRepository.SaveChanges();

            return new PayloadResponse
            {
                IsSuccess = true,
                PayloadType = "Passenger Update",
                Content = null,
                Message = "Passenger Update successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Passenger Update",
                Content = null,
                Message = $"Passenger Update become failed because {ex.Message}"
            };
        }
    }

    private bool IfDuplicateEmail(string emailAddress)
    {
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.EmailAddress == emailAddress);

        return user is not null;
    }

    public PayloadResponse GetPassengerById(string id)
    {
        var passenger = _userRepository
            .GetAll().Where(u => u.Id == id)
            .Include(p => p.Organization)
            .FirstOrDefault();

        if(passenger == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger Get",
                Content = new PassengerDto(),
                Message = "Passenger not found!"
            };
        }

        var cardData = _cardService.GetCardDataFromPassengerId(id);

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Passenger Get",
            Content = new PassengerDto()
            {
                Id = passenger.Id,
                Name = passenger.Name,
                DateOfBirth = passenger.DateOfBirth,
                MobileNumber = passenger.MobileNumber,
                EmailAddress = passenger.EmailAddress,
                Address = passenger.Address,
                Gender = passenger.Gender,
                UserType = passenger.UserType,
                ImageUrl = passenger.ImageUrl,
                PassengerId = passenger.PassengerId,
                OrganizationId = passenger.OrganizationId,
                Organization = passenger.Organization,
                CardNumber = cardData.CardNumber,
                Balance = cardData.Balance,
                CreateTime = passenger.CreateTime,
                LastModifiedTime = passenger.LastModifiedTime,
                Designation = passenger.Designation
            },
            Message = "Passenger data found!"
        };
    }

    public PayloadResponse UpdateCardNumber(CardNumberUpdateRequest model)
    {
        var passenger = _userRepository
            .GetConditional(p => p.Id == model.UserId);

        var passengerCard = _cardService
            .GetCardDataFromPassengerId(model.UserId);

        if (passengerCard == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Passenger is not assigned to any card!"
            };
        }

        if (passenger == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Passenger not found!"
            };
        }

        var cardValidity = _cardService.CheckCardValidity(model.CardNumber);

        if (!cardValidity.IsSuccess)
        {
            return cardValidity;
        }

        var previousCard = _cardService.GetCardDetailByCardNumber(passengerCard.CardNumber);
        previousCard.Status = CardStatus.Obsolete;
        _cardService.UpdateCard(previousCard);

        var newCard = _cardService.GetCardDetailByCardNumber(model.CardNumber);
        newCard.Status = CardStatus.InUse;
        newCard.Balance += previousCard.Balance;
        _cardService.UpdateCard(newCard);

        _cardService.UnmapUserWithPreviousCard(passenger.Id, previousCard.Id);
        _cardService.MapUserWithCard(passenger.Id, newCard.Id);
        _cardService.MapUserWithCardHistory(passenger.Id, previousCard.Id);

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Card Number changer for passenger",
            Message = "Card number has been updated!"
        };
    }

    public PayloadResponse GetAll(PassengerDataFilter filter)
    {
        try
        {
            var currentUser = _loggedInUserService
                .GetLoggedInUser();

            if (currentUser is null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Passenger",
                    Message = "Current User not found!"
                };
            }

            var condition = new List<string> { " u.UserType in ('Public', 'Private') " };
            var extraCondition = $@"ORDER BY CreateTime desc
                                    OFFSET ({filter.PageNo} - 1) * {filter.PageSize} ROWS
                                    FETCH NEXT {filter.PageSize} ROWS ONLY";

            if (!currentUser.IsSuperAdmin)
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Passenger",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (u.Name like '%{filter.SearchQuery}%' or u.MobileNumber like '%{filter.SearchQuery}%' or u.PassengerId like '%{filter.SearchQuery}%' or c.CardNumber like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" u.OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Users u", whereCondition);

            var passengerData = GetAllUserData(whereCondition, extraCondition);

            var allOrg = _organizationRepository.GetAll();

            foreach (var data in passengerData)
            {
                data.Organization = allOrg.FirstOrDefault(o => o.Id == data.OrganizationId);
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Passenger",
                Content = new { data = passengerData, rowCount },
                Message = "Passenger data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger",
                Message = $"Passenger fetching is failed because {ex.Message}!"
            };
        }
    }

    private List<PassengerDto> GetAllUserData(string whereCondition, string extraCondition)
    {
        var query = $@"
                    select u.*, c.CardNumber, c.Balance from Users u
                    left join PassengerCardMappings pcm on u.Id = pcm.UserId
                    left join Cards c on c.Id = pcm.CardId
                    {whereCondition} {extraCondition}";

        var data = _baseRepository.Query<PassengerDto>(query);

        return data;
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var passenger = _userRepository
                .GetConditional(u => u.Id == id);

            if (passenger == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Passenger",
                    Message = "Passenger not found"
                };
            }

            _userRepository.Delete(passenger);
            _userRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Passenger",
                Message = "Passenger has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger",
                Message = $"Passenger deletion is failed! because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetOnGoingTrip()
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();
            var card = _cardService.GetCardDataFromPassengerId(currentUser.Id);

            var trip = _tripRepository
                .GetAll()
                .Where(t => t.CardId == card.Id && t.IsRunning)
                .Include(t => t.Session)
                .Include(t => t.Card)
                .Include(t => t.Session.Bus)
                .Include(t => t.Session.Bus.Route)
                .Select(t => new OnGoingTripDto()
                {
                    TripId = t.Id,
                    BusName = t.Session.Bus.BusName,
                    BusNumber = t.Session.Bus.BusNumber,
                    CardId = t.CardId,
                    CardNumber = t.Card.CardNumber,
                    IsRunning = t.IsRunning,
                    PenaltyAmount = t.Session.Bus.Route.PenaltyAmount,
                    SessionId = t.SessionId,
                    StartingLatitude = t.StartingLatitude,
                    StartingLongitude = t.StartingLongitude,
                    TripStartPlace = t.Session.Bus.Route.TripStartPlace,
                    TripEndPlace = t.Session.Bus.Route.TripEndPlace,
                    TripStartTime = t.TripStartTime
                })
                .FirstOrDefault();

            if (trip == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Passenger",
                    Message = "No ongoing trip!"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Passenger",
                Content = trip,
                Message = "Ongoing trip found!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Passenger",
                Message = $"Ongoing trip fetch failed because {ex.Message}!"
            };
        }
    }

    private bool IfDuplicateUser(PassengerCreateRequest model)
    {
        User user;

        if (!string.IsNullOrEmpty(model.EmailAddress))
        {
            user = _userRepository
                .GetAll()
                .FirstOrDefault(u => u.MobileNumber == model.MobileNumber ||
                                     u.EmailAddress == model.EmailAddress);

            return user is not null;
        }

        user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == model.MobileNumber);

        return user is not null;
    }
    
    private bool IfDuplicateMobileNumber(string mobileNumber)
    {
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == mobileNumber);

        return user is not null;
    }
}