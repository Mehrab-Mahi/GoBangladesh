using GoBangladesh.Application.DTOs.TicketChecker;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Linq;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.DTOs.Staff;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GoBangladesh.Application.Services;

public class TicketCheckerService : ITicketCheckerService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;

    public TicketCheckerService(IRepository<User> userRepository,
        ILoggedInUserService loggedInUserService,
        ICommonService commonService)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
    }

    public PayloadResponse TicketCheckerCreate(TicketCheckerCreateRequest user)
    {
        if (IfDuplicateUser(user))
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Ticket Checker",
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
                UserType = UserTypes.TicketChecker,
                OrganizationId = user.OrganizationId
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
                PayloadType = "Ticket Checker",
                Content = null,
                Message = "Ticket Checker Creation has been successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Ticket Checker",
                Content = null,
                Message = $"Ticket Checker Creation become unsuccessful because {ex.Message}"
            };
        }
    }

    public PayloadResponse TicketCheckerUpdate(TicketCheckerUpdateRequest user)
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
                        PayloadType = "Ticket Checker",
                        Content = null,
                        Message = "User with the mobile number already exists!"
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
                        PayloadType = "Ticket Checker",
                        Content = null,
                        Message = "User with the email already exists!"
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
                PayloadType = "Ticket Checker",
                Content = null,
                Message = "Ticket Checker Update successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Ticket Checker",
                Content = null,
                Message = $"Ticket Checker Update become failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetById(string id)
    {
        var ticketChecker = _userRepository
            .GetAll().Where(u => u.Id == id)
            .Include(p => p.Organization)
            .FirstOrDefault();

        if (ticketChecker == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Ticket Checker",
                Content = new StaffDto(),
                Message = "Ticket Checker not found!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Ticket Checker",
            Content = new StaffDto()
            {
                Id = ticketChecker.Id,
                Name = ticketChecker.Name,
                DateOfBirth = ticketChecker.DateOfBirth,
                MobileNumber = ticketChecker.MobileNumber,
                EmailAddress = ticketChecker.EmailAddress,
                Address = ticketChecker.Address,
                Gender = ticketChecker.Gender,
                UserType = ticketChecker.UserType,
                ImageUrl = ticketChecker.ImageUrl,
                Organization = ticketChecker.Organization,
                OrganizationId = ticketChecker.OrganizationId,
                CreateTime = ticketChecker.CreateTime,
                LastModifiedTime = ticketChecker.LastModifiedTime
            },
            Message = "Ticket Checker not found!"
        };
    }

    public PayloadResponse GetAll(TicketCheckerDataFilter filter)
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
                    PayloadType = "Ticket Checker",
                    Message = "Current User not found!"
                };
            }

            var condition = new List<string> { " UserType = 'TicketChecker' " };
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
                        PayloadType = "Ticket Checker",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (Name like '%{filter.SearchQuery}%' or MobileNumber like '%{filter.SearchQuery}%' ) ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" OrganizationId = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Users", whereCondition);

            var finalQueryData = _commonService.GetFinalData<User>("Users", whereCondition, extraCondition);

            var userIds = finalQueryData.Select(q => q.Id).ToList();

            var staffData = _userRepository.GetAll()
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.Organization)
                .Select(staff => new StaffDto()
                {
                    Id = staff.Id,
                    Name = staff.Name,
                    DateOfBirth = staff.DateOfBirth,
                    MobileNumber = staff.MobileNumber,
                    EmailAddress = staff.EmailAddress,
                    Address = staff.Address,
                    Gender = staff.Gender,
                    UserType = staff.UserType,
                    ImageUrl = staff.ImageUrl,
                    Organization = staff.Organization,
                    Code = staff.Code,
                    OrganizationId = staff.OrganizationId,
                    CreateTime = staff.CreateTime,
                    LastModifiedTime = staff.LastModifiedTime
                })
                .OrderByDescending(s => s.CreateTime)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Ticket Checker",
                Content = new { data = staffData, rowCount },
                Message = "Ticket Checker data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Ticket Checker",
                Message = $"Ticket Checker fetching is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var ticketChecker = _userRepository
                .GetConditional(u => u.Id == id);

            if (ticketChecker == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Ticket Checker",
                    Message = "Ticket Checker not found"
                };
            }

            _userRepository.Delete(ticketChecker);
            _userRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Ticket Checker",
                Message = "Ticket Checker has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Ticket Checker",
                Message = $"Ticket Checker deletion is failed! because {ex.Message}"
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

    private bool IfDuplicateMobileNumber(string mobileNumber)
    {
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == mobileNumber);

        return user is not null;
    }

    private bool IfDuplicateUser(TicketCheckerCreateRequest model)
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
}