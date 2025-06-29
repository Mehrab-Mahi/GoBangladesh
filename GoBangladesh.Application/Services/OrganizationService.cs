using System;
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

    public OrganizationService(IRepository<Organization> organizationRepository)
    {
        _organizationRepository = organizationRepository;
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

    public PayloadResponse GetAll()
    {
        try
        {
            var organizationList = _organizationRepository
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