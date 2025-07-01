using GoBangladesh.Application.DTOs.Organization;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IOrganizationService
{
    PayloadResponse OrganizationInsert(OrganizationCreateRequest model);
    PayloadResponse OrganizationUpdate(OrganizationUpdateRequest model);
    PayloadResponse GetById(string id);
    PayloadResponse GetAll(int pageNo, int pageSize);
    PayloadResponse Delete(string id);
}