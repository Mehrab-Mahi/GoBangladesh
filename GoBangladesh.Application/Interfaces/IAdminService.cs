using GoBangladesh.Application.DTOs.Admin;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IAdminService
{
    PayloadResponse AdminInsert(AdminCreateRequest model);
    PayloadResponse UpdateAdmin(AdminUpdateRequest model);
    PayloadResponse GetAdminById(string id);
    PayloadResponse GetAll(AdminDataFilter filter);
    PayloadResponse Delete(string id);
}