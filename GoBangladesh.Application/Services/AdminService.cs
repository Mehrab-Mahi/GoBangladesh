using GoBangladesh.Application.DTOs.Admin;
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

public class AdminService : IAdminService
{
    private readonly IRepository<User> _userRepository;
    private readonly ILoggedInUserService _loggedInUserService;
    private readonly ICommonService _commonService;

    public AdminService(IRepository<User> userRepository, 
        ILoggedInUserService loggedInUserService,
        ICommonService commonService)
    {
        _userRepository = userRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
    }

    public PayloadResponse AdminInsert(AdminCreateRequest user)
    {
        if (IfDuplicateUser(user))
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Admin Creation",
                Content = null,
                Message = "Duplicate user!"
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
                UserType = UserTypes.Admin,
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
                PayloadType = "Admin",
                Content = null,
                Message = "Admin Creation has been successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Admin",
                Content = null,
                Message = $"Admin Creation become unsuccessful because {ex.Message}"
            };
        }
    }

    private bool IfDuplicateUser(AdminCreateRequest model)
    {
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == model.MobileNumber
                                 || u.EmailAddress == model.EmailAddress);

        return user is not null;
    }

    public PayloadResponse UpdateAdmin(AdminUpdateRequest user)
    {
        var model = _userRepository
            .GetConditional(u => u.Id == user.Id);
        try
        {
            if (user.MobileNumber != model.MobileNumber)
            {
                if (IfDuplicateMobileNumber(user.MobileNumber))
                {
                    return new PayloadResponse
                    {
                        IsSuccess = false,
                        PayloadType = "Admin Update",
                        Content = null,
                        Message = "User with the mobile number already exists!"
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
                PayloadType = "Admin",
                Content = null,
                Message = "Admin Update successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                PayloadType = "Admin Update",
                Content = null,
                Message = $"Admin Update is failed because {ex.Message}"
            };
        }
    }

    private bool IfDuplicateMobileNumber(string mobileNumber)
    {
        var user = _userRepository
            .GetAll()
            .FirstOrDefault(u => u.MobileNumber == mobileNumber);

        return user is not null;
    }

    public PayloadResponse GetAdminById(string id)
    {
        var admin = _userRepository
            .GetAll().Where(u => u.Id == id)
            .Include(p => p.Organization)
            .FirstOrDefault();

        if (admin == null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Admin",
                Content = new AdminDto(),
                Message = "Admin not found!"
            };
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            PayloadType = "Admin",
            Content = new AdminDto()
            {
                Id = admin.Id,
                Name = admin.Name,
                DateOfBirth = admin.DateOfBirth,
                MobileNumber = admin.MobileNumber,
                EmailAddress = admin.EmailAddress,
                Address = admin.Address,
                Gender = admin.Gender,
                UserType = admin.UserType,
                ImageUrl = admin.ImageUrl,
                OrganizationId = admin.OrganizationId,
                Organization = admin.Organization
            },
            Message = "Admin not found!"
        };
    }

    public PayloadResponse GetAll(int pageNo, int pageSize)
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
                    PayloadType = "Admin",
                    Message = "Current User not found!"
                };
            }

            List<AdminDto> adminData;

            if (currentUser.IsSuperAdmin)
            {
                adminData = _userRepository
                    .GetAll()
                    .Include(u => u.Organization)
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new AdminDto()
                    {
                        Id = u.Id,
                        Name = u.Name,
                        DateOfBirth = u.DateOfBirth,
                        MobileNumber = u.MobileNumber,
                        EmailAddress = u.EmailAddress,
                        Address = u.Address,
                        Gender = u.Gender,
                        UserType = u.UserType,
                        ImageUrl = u.ImageUrl,
                        OrganizationId = u.OrganizationId,
                        Organization = u.Organization
                    })
                    .ToList();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Admin",
                    Content = adminData,
                    Message = "Admin data fetch is successful"
                };
            }

            if (string.IsNullOrEmpty(currentUser.OrganizationId))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Admin",
                    Message = "Current User is not associated with any organization!"
                };
            }

            adminData = _userRepository
                .GetAll()
                .Where(u => u.OrganizationId == currentUser.OrganizationId)
                .Include(u => u.Organization)
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminDto()
                {
                    Id = u.Id,
                    Name = u.Name,
                    DateOfBirth = u.DateOfBirth,
                    MobileNumber = u.MobileNumber,
                    EmailAddress = u.EmailAddress,
                    Address = u.Address,
                    Gender = u.Gender,
                    UserType = u.UserType,
                    ImageUrl = u.ImageUrl,
                    OrganizationId = u.OrganizationId,
                    Organization = u.Organization
                })
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Admin",
                Content = adminData,
                Message = "Admin data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Admin",
                Message = $"Admin fetching is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var admin = _userRepository
                .GetConditional(u => u.Id == id);

            if (admin == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Admin",
                    Message = "Admin not found"
                };
            }

            _userRepository.Delete(admin);
            _userRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Admin",
                Message = "Admin has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Admin",
                Message = $"Admin deletion is failed! because {ex.Message}"
            };
        }
    }
}