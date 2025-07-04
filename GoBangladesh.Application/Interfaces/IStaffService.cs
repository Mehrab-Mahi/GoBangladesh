using GoBangladesh.Application.DTOs.Session;
using GoBangladesh.Application.DTOs.Staff;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IStaffService
{
    PayloadResponse StaffInsert(StaffCreateRequest model);
    PayloadResponse UpdateStaff(StaffUpdateRequest model);
    PayloadResponse GetStaffById(string id);
    PayloadResponse GetAll(StaffDataFilter filter);
    PayloadResponse Delete(string id);
}