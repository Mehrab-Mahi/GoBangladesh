﻿using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.Util;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class PassengerService : IPassengerService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IRepository<Trip> _tripRepository;
    private readonly ICardService _cardService;

    public PassengerService(IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService, 
        IRepository<Trip> tripRepository,
        ICardService cardService)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _tripRepository = tripRepository;
        _cardService = cardService;
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

        if (string.IsNullOrEmpty(user.UserType))
        {
            var card = _cardService.GetCardDetailByCardNumber(user.CardNumber);
            user.CardNumber = card.CardNumber;
            user.OrganizationId = string.IsNullOrEmpty(user.OrganizationId) ? card.OrganizationId : user.OrganizationId;
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
                CardNumber = user.CardNumber
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

            if (user.UserType == UserTypes.Public)
            {
                _cardService.UpdateCardStatus(user.CardNumber, CardStatus.InUse);
            }

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
                CardNumber = passenger.CardNumber,
                Balance = passenger.Balance,
                CreateTime = passenger.CreateTime,
                LastModifiedTime = passenger.LastModifiedTime
            },
            Message = "Passenger not found!"
        };
    }

    public PayloadResponse UpdateCardNumber(CardNumberUpdateRequest model)
    {
        var passenger = _userRepository
            .GetConditional(p => p.Id == model.UserId);

        if (passenger == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Passenger not found!"
            };
        }

        if (passenger.UserType == UserTypes.Private)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Passenger is not allowed to change his card number! Please contact with the administrator!"
            };
        }

        var cardValidity = _cardService.CheckCardValidity(model.CardNumber);

        if (!cardValidity.IsSuccess)
        {
            return cardValidity;
        }

        var previousCard = _cardService.GetCardDetailByCardNumber(passenger.CardNumber);
        previousCard.Status = CardStatus.Obsolete;
        _cardService.UpdateCard(previousCard);

        var newCard = _cardService.GetCardDetailByCardNumber(model.CardNumber);
        newCard.Status = CardStatus.InUse;
        _cardService.UpdateCard(newCard);

        passenger.CardNumber = model.CardNumber;
        _userRepository.Update(passenger);
        _userRepository.SaveChanges();

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

            var condition = new List<string> { " UserType in ('Public', 'Private') " };
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
                condition.Add($" (Name like '%{filter.SearchQuery}%' or MobileNumber like '%{filter.SearchQuery}%' or PassengerId like '%{filter.SearchQuery}%' or CardNumber like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Users", whereCondition);

            var finalQueryData = _commonService.GetFinalData<User>("Users", whereCondition, extraCondition);

            var userIds = finalQueryData.Select(q => q.Id).ToList();

            var passengerData = _userRepository.GetAll()
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.Organization)
                .Select(passenger => new PassengerDto()
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
                    CardNumber = passenger.CardNumber,
                    Balance = passenger.Balance,
                    CreateTime = passenger.CreateTime,
                    LastModifiedTime = passenger.LastModifiedTime
                })
                .OrderByDescending(p => p.CreateTime)
                .ToList();

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

            var trip = _tripRepository
                .GetAll()
                .Where(t => t.PassengerId == currentUser.Id && t.IsRunning)
                .Include(t => t.Session)
                .Include(t => t.Session.Bus)
                .Include(t => t.Session.Bus.Route)
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
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == model.MobileNumber 
                                 || u.CardNumber == model.CardNumber);

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