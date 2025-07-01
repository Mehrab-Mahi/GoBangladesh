using GoBangladesh.Application.DTOs.Staff;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IStaffService
{
    PayloadResponse StaffInsert(StaffCreateRequest model);
    PayloadResponse UpdateStaff(StaffUpdateRequest model);
    PayloadResponse GetStaffById(string id);
    PayloadResponse MapStaffWithBus(StaffBusMappingDto staffBusMapping);
    PayloadResponse GetAll(int pageNo, int pageSize);
    PayloadResponse Delete(string id);
}