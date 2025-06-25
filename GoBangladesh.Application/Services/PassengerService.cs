using GoBangladesh.Application.DTOs.Passenger;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class PassengerService : IPassengerService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;

    public PassengerService(IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
    }

    public PayloadResponse PassengerInsert(PassengerCreateRequest user)
    {
        if (IfDuplicateUser(user.MobileNumber))
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Passenger Creation",
                Content = null,
                Message = "User with this mobile number already exists!"
            };
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
                OrganizationId = user.OrganizationId
            };

            var currentUser = _loggedInUserService.GetLoggedInUser();

            model.PasswordHash = _commonService.GetPasswordHash(user.Password);
            model.ImageUrl = _commonService.UploadAndGetImageUrl(user.ProfilePicture, "ProfilePicture");
            model.CreatedBy = currentUser is null ? "" : currentUser.Id;
            model.LastModifiedBy = currentUser is null ? "" : currentUser.Id;

            _userRepository.InsertWithUserData(model);
            _userRepository.SaveChanges();

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
                if (IfDuplicateUser(user.MobileNumber))
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

            model.DateOfBirth = user.DateOfBirth;
            model.MobileNumber = user.MobileNumber;
            model.EmailAddress = user.EmailAddress;
            model.Address = user.Address;
            model.Gender = user.Gender;
            model.PassengerId = user.PassengerId;
            model.OrganizationId = user.OrganizationId;

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
                Organization = passenger.Organization
            },
            Message = "Passenger not found!"
        };
    }

    private bool IfDuplicateUser(string mobileNumber)
    {
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == mobileNumber);

        return user is not null;
    }
}