using GoBangladesh.Application.DTOs.Agent;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoBangladesh.Application.Services;

public class AgentService : IAgentService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IBaseRepository _baseRepository;

    public AgentService(IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService, IBaseRepository baseRepository)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _baseRepository = baseRepository;
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
                UserType = UserTypes.Agent,
                OrganizationId = user.OrganizationId,
                Serial = serial,
                Code = $"AGT-{serial:D6}"
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

            model.Name = user.Name;
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
                OrganizationId = agent.OrganizationId,
                Organization = agent.Organization,
                Code = agent.Code,
                CreateTime = agent.CreateTime,
                LastModifiedTime = agent.LastModifiedTime
            },
            Message = "Agent not found!"
        };
    }

    public PayloadResponse GetAll(AgentDataFilter filter)
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
                    PayloadType = "Agent",
                    Message = "Current User not found!"
                };
            }

            var condition = new List<string> { " UserType = 'Agent' " };
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
                        PayloadType = "Agent",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }
            
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (Name like '%{filter.SearchQuery}%' or Code like '%{filter.SearchQuery}%' or MobileNumber like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Users", whereCondition);

            var finalQueryData = _commonService.GetFinalData<User>("Users", whereCondition, extraCondition);

            var userIds = finalQueryData.Select(q => q.Id).ToList();

            var agentData = _userRepository.GetAll()
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.Organization)
                .Select(agent => new AgentDto()
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
                    OrganizationId = agent.OrganizationId,
                    Organization = agent.Organization,
                    Code = agent.Code,
                    CreateTime = agent.CreateTime,
                    LastModifiedTime = agent.LastModifiedTime
                })
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Agent",
                Content = new { data = agentData, rowCount },
                Message = "Agent data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Agent",
                Message = $"Agent fetching is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var agent = _userRepository
                .GetConditional(u => u.Id == id);

            if (agent == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Agent",
                    Message = "Agent not found"
                };
            }

            _userRepository.Delete(agent);
            _userRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Agent",
                Message = "Agent has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Agent",
                Message = $"Agent deletion is failed! because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetRechargeData(string id)
    {
        try{
            var query = $@"SELECT SUM(CASE
                                   WHEN CreateTime >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()), 0)
                                       AND CreateTime < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()) + 1, 0)
                                       THEN 1
                                   ELSE 0 END) AS ThisMonthTransactionCount,
                           SUM(CASE
                                   WHEN CreateTime >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()), 0)
                                       AND CreateTime < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()) + 1, 0)
                                       THEN Amount
                                   ELSE 0 END) AS ThisMonthTotalAmount,
                           SUM(CASE
                                   WHEN CreateTime >= CAST(GETUTCDATE() AS DATE)
                                       AND CreateTime < DATEADD(DAY, 1, CAST(GETUTCDATE() AS DATE))
                                       THEN 1
                                   ELSE 0 END) AS TodayTransactionCount,
                           SUM(CASE
                                   WHEN CreateTime >= CAST(GETUTCDATE() AS DATE)
                                       AND CreateTime < DATEADD(DAY, 1, CAST(GETUTCDATE() AS DATE))
                                       THEN Amount
                                   ELSE 0 END) AS TodayTotalAmount
                    FROM Transactions
                    WHERE TransactionType = 'Recharge'
                      AND CreatedBy = '{id}'";

        var rechargeData = _baseRepository
            .Query<RechargeInfo>(query)
            .FirstOrDefault();

        if (rechargeData == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Organization",
                Message = "Organization not found"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Agent",
            Content = rechargeData,
            Message = "Agent recharge data has been sent!"
        };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Agent",
                Message = $"Agent recharge data fetching has been failed because {ex.Message}"
            };
        }
    } 
}