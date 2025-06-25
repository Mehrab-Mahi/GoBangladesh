using GoBangladesh.Application.DTOs.Agent;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GoBangladesh.Application.Services;

public class AgentService : IAgentService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;

    public AgentService(IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
    }
    public PayloadResponse AgentInsert(AgentCreateRequest user)
    {
        if (IfDuplicateUser(user.MobileNumber))
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Agent Creation",
                Content = null,
                Message = "User with this mobile number already exists!"
            };
        }

        try
        {
            var serial = GetSerialNumber();

            var model = new User()
            {
                Name = user.Name,
                EmailAddress = user.EmailAddress,
                DateOfBirth = user.DateOfBirth,
                MobileNumber = user.MobileNumber,
                Address = user.Address,
                Gender = user.Gender,
                UserType = user.UserType,
                OrganizationId = user.OrganizationId,
                Serial = serial,
                Code = serial.ToString("D6")
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
                PayloadType = "Agent Creation",
                Content = null,
                Message = "Agent Creation has been successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Agent Creation",
                Content = null,
                Message = $"Agent Creation become unsuccessful because {ex.Message}"
            };
        }
    }

    private int GetSerialNumber()
    {
        var maxSerial = _userRepository.GetAll().Max(s => s.Serial);
        return maxSerial + 1;
    }

    private bool IfDuplicateUser(string mobileNumber)
    {
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == mobileNumber);

        return user is not null;
    }

    public PayloadResponse UpdateAgent(AgentUpdateRequest user)
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
                        PayloadType = "Agent Update",
                        Content = null,
                        Message = "Agent with the mobile number already exists!"
                    };
                }
            }

            model.DateOfBirth = user.DateOfBirth;
            model.MobileNumber = user.MobileNumber;
            model.EmailAddress = user.EmailAddress;
            model.Address = user.Address;
            model.Gender = user.Gender;
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
                PayloadType = "Agent Update",
                Content = null,
                Message = "Agent Update successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Agent Update",
                Content = null,
                Message = $"Agent Update become failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetAgentById(string id)
    {
        var agent = _userRepository
            .GetAll().Where(u => u.Id == id)
            .Include(p => p.Organization)
            .FirstOrDefault();

        if (agent == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Agent Get",
                Content = new AgentDto(),
                Message = "Agent not found!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Agent Get",
            Content = new AgentDto()
            {
                Id = agent.Id,
                Name = agent.Name,
                DateOfBirth = agent.DateOfBirth,
                MobileNumber = agent.MobileNumber,
                EmailAddress = agent.EmailAddress,
                Address = agent.Address,
                Gender = agent.Gender,
                UserType = agent.UserType,
                ImageUrl = agent.ImageUrl,
                Organization = agent.Organization,
                Code = agent.Code
            },
            Message = "Agent not found!"
        };
    }
}