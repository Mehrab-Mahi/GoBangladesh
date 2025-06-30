using GoBangladesh.Application.DTOs.Staff;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.Util;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GoBangladesh.Application.Services;

public class StaffService : IStaffService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;
    private readonly IRepository<StaffBusMapping> _staffBusMappingRepository;

    public StaffService(IRepository<User> userRepository, 
        ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        IRepository<StaffBusMapping> staffBusMappingRepository)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _staffBusMappingRepository = staffBusMappingRepository;
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
            var model = new User()
            {
                Name = user.Name,
                EmailAddress = user.EmailAddress,
                DateOfBirth = user.DateOfBirth,
                MobileNumber = user.MobileNumber,
                Address = user.Address,
                Gender = user.Gender,
                UserType = UserTypes.Staff,
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
                Organization = staff.Organization
            },
            Message = "Passenger not found!"
        };
    }

    public PayloadResponse MapStaffWithBus(StaffBusMappingDto staffBusMapping)
    {
        try
        {
            var mapping = _staffBusMappingRepository
                .GetConditional(b => b.BusId == staffBusMapping.BusId);

            if (mapping == null)
            {
                _staffBusMappingRepository.Insert(new StaffBusMapping()
                {
                    BusId = staffBusMapping.BusId,
                    UserId = staffBusMapping.UserId
                });
            }
            else
            {
                mapping.UserId = staffBusMapping.UserId;

                _staffBusMappingRepository.Update(mapping);
            }

            _staffBusMappingRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Staff Bus Mapping",
                Message = "Staff has been mapped with bus successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Staff Bus Mapping",
                Message = $"Staff bus mapping failed because {ex.Message}"
            };
        }
    }
}