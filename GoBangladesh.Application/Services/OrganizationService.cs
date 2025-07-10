using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.DTOs;
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
    private readonly ICommonService _commonService;
    private readonly IBaseRepository _baseRepository;

    public OrganizationService(IRepository<Organization> organizationRepository, 
        ILoggedInUserService loggedInUserService,
        ICommonService commonService,
        IBaseRepository baseRepository)
    {
        _organizationRepository = organizationRepository;
        _loggedInUserService = loggedInUserService;
        _commonService = commonService;
        _baseRepository = baseRepository;
    }

    public PayloadResponse OrganizationInsert(OrganizationCreateRequest model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (!currentUser.IsSuperAdmin)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Organization",
                    Message = "User is not authorized to create an organization!"
                };
            }

            var duplicateVerification = IfDuplicateOrganizationData(model);

            if (!duplicateVerification.IsSuccess)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Organization",
                    Message = $"{duplicateVerification.Message}"
                };
            }

            var organization = new Organization()
            {
                Name = model.Name,
                Code = model.Code,
                FocalPerson = model.FocalPerson,
                Email = model.Email,
                MobileNumber = model.MobileNumber,
                Designation = model.Designation,
                OrganizationType = model.OrganizationType
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

    private PayloadResponse IfDuplicateOrganizationData(OrganizationCreateRequest model)
    {
        var organization = _organizationRepository.GetAll();

        if (organization.FirstOrDefault(o => o.Code == model.Code) != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Duplicate code found for organization"
            };
        }

        if (organization.FirstOrDefault(o => o.MobileNumber == model.MobileNumber) != null)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                Message = "Duplicate mobile number found for organization"
            };
        }

        if (!string.IsNullOrEmpty(model.Email))
        {
            if (organization.FirstOrDefault(o => o.Email == model.Email) != null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Duplicate email found for organization"
                };
            }
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            Message = "No Duplicates!"
        };
    }

    public PayloadResponse OrganizationUpdate(OrganizationUpdateRequest model)
    {
        try
        {
            var currentUser = _loggedInUserService.GetLoggedInUser();

            if (!currentUser.IsSuperAdmin)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Organization",
                    Message = "User is not authorized to update an organization!"
                };
            }

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

            if (organization.Code != model.Code ||
                organization.Email != model.Email ||
                organization.MobileNumber != model.MobileNumber)
            {
                var duplicateVerification = IfDuplicateOrganizationDataWhileUpdate(organization, model);

                if (!duplicateVerification.IsSuccess)
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Organization",
                        Message = $"{duplicateVerification.Message}"
                    };
                }
            }

            organization.Name = model.Name;
            organization.Code = model.Code;
            organization.FocalPerson = model.FocalPerson;
            organization.Email = model.Email;
            organization.MobileNumber = model.MobileNumber;
            organization.Designation = model.Designation;
            organization.OrganizationType = model.OrganizationType;

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

    private PayloadResponse IfDuplicateOrganizationDataWhileUpdate(Organization organization,
        OrganizationUpdateRequest model)
    {
        var organizationList = _organizationRepository
            .GetAll()
            .Where(o => o.Id != organization.Id);

        if (organization.Code != model.Code)
        {
            if (organizationList.FirstOrDefault(o => o.Code == model.Code) != null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Duplicate code found for organization"
                };
            }
        }

        if (organization.Email != model.Email && !string.IsNullOrEmpty(model.Email))
        {
            if (organizationList.FirstOrDefault(o => o.Email == model.Email) != null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Duplicate email found for organization"
                };
            }
        }

        if (organization.MobileNumber != model.MobileNumber)
        {
            if (organizationList.FirstOrDefault(o => o.MobileNumber == model.MobileNumber) != null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = "Duplicate mobile number found for organization"
                };
            }
        }

        return new PayloadResponse()
        {
            IsSuccess = true,
            Message = "No Duplicates!"
        };
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var query = $@"
                        select o.*,
                               count(distinct b.Id)  as TotalBus,
                               count(distinct u.Id)  as TotalStaff,
                               count(distinct u1.Id) as TotalAgent,
                               count(distinct u2.Id) as TotalPassenger
                        from Organizations o
                                 left join Buses b on o.Id = b.OrganizationId
                                 left join Users u on o.Id = u.OrganizationId and u.UserType = 'Staff'
                                 left join Users u1 on o.Id = u1.OrganizationId and u1.UserType = 'Agent'
                                 left join Users u2 on o.Id = u2.OrganizationId and u2.UserType in ('Public', 'Private')
                        where o.Id = '{id}'
                        group by o.Id, o.Name, o.FocalPerson, o.Email, o.MobileNumber, o.CreateTime, o.LastModifiedTime, o.CreatedBy,
                                 o.LastModifiedBy, o.IsDeleted, o.Code, o.Designation, o.OrganizationType";

            var organizationData = _baseRepository
                .Query<OrganizationDataDto>(query)
                .FirstOrDefault();

            if (organizationData == null)
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
                Content = organizationData,
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

    public PayloadResponse GetAll(OrganizationDataFilter filter)
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

            var condition = new List<string>();
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
                        PayloadType = "Organization",
                        Message = "Current User is not associated with any organization!"
                    };
                }

                filter.OrganizationId = currentUser.OrganizationId;
            }

            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                condition.Add($" (Name like '%{filter.SearchQuery}%' or FocalPerson like '%{filter.SearchQuery}%' or Email like '%{filter.SearchQuery}%' or MobileNumber like '%{filter.SearchQuery}%') ");
            }

            if (!string.IsNullOrEmpty(filter.OrganizationId))
            {
                condition.Add($" Id = '{filter.OrganizationId}'");
            }

            var whereCondition = _commonService.GenerateWhereConditionFromConditionList(condition);

            var rowCount = _commonService.GetRowCountForData("Organizations", whereCondition);

            var finalQueryData = _commonService.GetFinalData<Organization>("Organizations", whereCondition, extraCondition);

            var organizationIds = finalQueryData.Select(q => q.Id).ToList();

            var organizationData = _organizationRepository.GetAll()
                .Where(u => organizationIds.Contains(u.Id))
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Organization",
                Content = new { data = organizationData, rowCount },
                Message = "Organization data fetch is successful"
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

    public PayloadResponse GetAllForSuperAdmin()
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

            List<ValueLabel> organizationData;

            if (currentUser.IsSuperAdmin)
            {
                organizationData = _organizationRepository
                    .GetAll()
                    .Select(o => new ValueLabel()
                    {
                        Value = o.Id,
                        Label = o.Name
                    })
                    .ToList();
            }
            else
            {
                if (string.IsNullOrEmpty(currentUser.OrganizationId))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Organization",
                        Message = "User is not from any organization!"
                    };
                }

                organizationData = _organizationRepository
                    .GetAll()
                    .Where(org => org.Id == currentUser.OrganizationId)
                    .Select(o => new ValueLabel()
                    {
                        Value = o.Id,
                        Label = o.Name
                    })
                    .ToList();
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Organization",
                Content = organizationData,
                Message = "Organization data fetch is successful"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Organization",
                Message = $"Organization data fetch failed because {ex.Message}"
            };
        }
    }
}