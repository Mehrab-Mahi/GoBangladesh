using GoBangladesh.Application.DTOs.Staff;
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

public class StaffService : IStaffService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;

    public StaffService(IRepository<User> userRepository, 
        ILoggedInUserService loggedInUserService,
        ICommonService commonService)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
    }

    public PayloadResponse StaffInsert(StaffCreateRequest user)
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
            var serial = GetSerialNumber();

            var model = new User()
            {
                Name = user.Name,
                EmailAddress = user.EmailAddress,
                DateOfBirth = user.DateOfBirth,
                MobileNumber = user.MobileNumber,
                Address = user.Address,
                Gender = user.Gender,
                UserType = UserTypes.Staff,
                OrganizationId = user.OrganizationId,
                Serial = serial,
                Code = $"STF-{serial:D6}"
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

    public PayloadResponse UpdateStaff(StaffUpdateRequest user)
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
                        PayloadType = "Staff Update",
                        Content = null,
                        Message = "Staff with the mobile number already exists!"
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
                PayloadType = "Staff Update",
                Content = null,
                Message = "Staff Update successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Staff Update",
                Content = null,
                Message = $"Staff Update become failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetStaffById(string id)
    {
        var staff = _userRepository
            .GetAll().Where(u => u.Id == id)
            .Include(p => p.Organization)
            .FirstOrDefault();

        if (staff == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Staff Get",
                Content = new StaffDto(),
                Message = "Staff not found!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Staff Get",
            Content = new StaffDto()
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
            },
            Message = "Passenger not found!"
        };
    }

    public PayloadResponse GetAll(StaffDataFilter filter)
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
                    PayloadType = "Staff",
                    Message = "Current User not found!"
                };
            }

            var condition = new List<string> { " UserType = 'Staff' " };
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
                        PayloadType = "Staff",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (Name like '%{filter.SearchQuery}%' or MobileNumber like '%{filter.SearchQuery}%' or Code like '%{filter.SearchQuery}%') ");
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
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Staff",
                Content = new { data = staffData, rowCount },
                Message = "Staff data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Staff",
                Message = $"Staff fetching is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var staff = _userRepository
                .GetConditional(u => u.Id == id);

            if (staff == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Staff",
                    Message = "Staff not found"
                };
            }

            _userRepository.Delete(staff);
            _userRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Staff",
                Message = "Staff has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Staff",
                Message = $"Staff deletion is failed! because {ex.Message}"
            };
        }
    }
}