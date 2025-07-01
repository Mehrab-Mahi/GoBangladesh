using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs.Organization;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class OrganizationService : IOrganizationService
{
    private readonly IRepository<Organization> _organizationRepository;
    private readonly ILoggedInUserService _loggedInUserService;

    public OrganizationService(IRepository<Organization> organizationRepository, 
        ILoggedInUserService loggedInUserService)
    {
        _organizationRepository = organizationRepository;
        _loggedInUserService = loggedInUserService;
    }

    public PayloadResponse OrganizationInsert(OrganizationCreateRequest model)
    {
        try
        {
            if (IfDuplicateOrganizationCode(model.Code))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Organization",
                    Message = "Duplicate code found for organization"
                };
            }

            var organization = new Organization()
            {
                Name = model.Name,
                Code = model.Code,
                FocalPerson = model.FocalPerson,
                Email = model.Email,
                MobileNumber = model.MobileNumber
            };

            _organizationRepository.Insert(organization);
            _organizationRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Organization",
                Message = "Organization has been inserted successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Organization",
                Message = $"Organization creation is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse OrganizationUpdate(OrganizationUpdateRequest model)
    {
        try
        {
            var organization = _organizationRepository
                .GetConditional(o => o.Id == model.Id);

            if (organization == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Organization",
                    Message = "Organization not found!"
                };
            }

            if (organization.Code != model.Code)
            {
                if (IfDuplicateOrganizationCode(model.Code))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Organization",
                        Message = "Duplicate code found for organization"
                    };
                }
            }

            organization.Name = model.Name;
            organization.Code = model.Code;
            organization.FocalPerson = model.FocalPerson;
            organization.Email = model.Email;
            organization.MobileNumber = model.MobileNumber;

            _organizationRepository.Update(organization);
            _organizationRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Organization",
                Message = "Organization has been updated successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Organization",
                Message = $"Organization update is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var organization = _organizationRepository
                .GetConditional(o => o.Id == id);

            if (organization == null)
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
                PayloadType = "Organization",
                Content = organization,
                Message = "Organization has been found"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Organization",
                Message = $"Organization fetching has been failed because {ex.Message}"
            };
        }
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
                    PayloadType = "Organization",
                    Message = "Current User not found!"
                };
            }

            List<Organization> organizationList;

            if (currentUser.IsSuperAdmin)
            {
                organizationList = _organizationRepository
                    .GetAll()
                    .ToList();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Organization",
                    Content = organizationList,
                    Message = "Organization has been sent successfully"
                };
            }

            if (string.IsNullOrEmpty(currentUser.OrganizationId))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Organization",
                    Message = "Current User is not associated with any organization!"
                };
            }

            organizationList = _organizationRepository
                .GetAll()
                .Where(o => o.Id == currentUser.OrganizationId)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Organization",
                Content = organizationList,
                Message = "Organization has been sent successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Organization",
                Message = $"Organization fetching has been failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var organization = _organizationRepository
                .GetConditional(o => o.Id == id);

            if (organization == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Organization",
                    Message = "Organization not found"
                };
            }

            _organizationRepository.Delete(organization);
            _organizationRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Organization",
                Message = "Organization has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Organization",
                Message = $"Organization deletion is failed! because {ex.Message}"
            };
        }
    }

    private bool IfDuplicateOrganizationCode(string code)
    {
        var organization = _organizationRepository
            .GetConditional(o => o.Code == code);

        return organization is not null;
    }
}