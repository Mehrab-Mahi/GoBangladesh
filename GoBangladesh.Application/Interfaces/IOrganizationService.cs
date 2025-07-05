using GoBangladesh.Application.DTOs.Organization;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Enums;

namespace GoBangladesh.Application.Interfaces;

public interface IOrganizationService
{
    PayloadResponse OrganizationInsert(OrganizationCreateRequest model);
    PayloadResponse OrganizationUpdate(OrganizationUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse GetAll(OrganizationDataFilter filter);
    PayloadResponse Delete(string id);
    PayloadResponse GetAllForSuperAdmin();
}